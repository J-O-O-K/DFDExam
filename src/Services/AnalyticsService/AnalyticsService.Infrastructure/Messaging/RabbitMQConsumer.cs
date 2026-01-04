using System.Text;
using System.Text.Json;
using AnalyticsService.Application.Features.Events.Commands.LogTaskEvent;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskManagement.Contracts.Events;

namespace AnalyticsService.Infrastructure.Messaging;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "task_events";

    public RabbitMQConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RabbitMQConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName, 
                type: ExchangeType.Topic, 
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var queueName = "analytics_service_queue";
            await _channel.QueueDeclareAsync(
                queue: queueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
            
            await _channel.QueueBindAsync(
                queue: queueName, 
                exchange: ExchangeName, 
                routingKey: "task.*",
                arguments: null,
                cancellationToken: cancellationToken);

            _logger.LogInformation("RabbitMQ Consumer connected and listening for task events");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel is not initialized");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("Received message from {RoutingKey}: {Message}", routingKey, message);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                var metadata = new Dictionary<string, object>();

                switch (routingKey)
                {
                    case "task.created":
                        var createdEvent = JsonSerializer.Deserialize<TaskCreatedEvent>(message);
                        if (createdEvent != null)
                        {
                            metadata["title"] = createdEvent.Title;
                            metadata["priority"] = createdEvent.Priority;
                            metadata["createdAt"] = createdEvent.CreatedAt;

                            await mediator.Send(new LogTaskEventCommand
                            {
                                EventType = "TaskCreated",
                                TaskId = createdEvent.TaskId,
                                Metadata = metadata
                            }, stoppingToken);
                        }
                        break;

                    case "task.updated":
                        var updatedEvent = JsonSerializer.Deserialize<TaskUpdatedEvent>(message);
                        if (updatedEvent != null)
                        {
                            if (updatedEvent.Title != null) metadata["title"] = updatedEvent.Title;
                            if (updatedEvent.Status != null) metadata["status"] = updatedEvent.Status;
                            metadata["updatedAt"] = updatedEvent.UpdatedAt;

                            await mediator.Send(new LogTaskEventCommand
                            {
                                EventType = "TaskUpdated",
                                TaskId = updatedEvent.TaskId,
                                Metadata = metadata
                            }, stoppingToken);
                        }
                        break;

                    case "task.completed":
                        var completedEvent = JsonSerializer.Deserialize<TaskCompletedEvent>(message);
                        if (completedEvent != null)
                        {
                            metadata["completedAt"] = completedEvent.CompletedAt;

                            await mediator.Send(new LogTaskEventCommand
                            {
                                EventType = "TaskCompleted",
                                TaskId = completedEvent.TaskId,
                                Metadata = metadata
                            }, stoppingToken);
                        }
                        break;

                    case "task.deleted":
                        var deletedEvent = JsonSerializer.Deserialize<TaskDeletedEvent>(message);
                        if (deletedEvent != null)
                        {
                            metadata["deletedAt"] = deletedEvent.DeletedAt;

                            await mediator.Send(new LogTaskEventCommand
                            {
                                EventType = "TaskDeleted",
                                TaskId = deletedEvent.TaskId,
                                Metadata = metadata
                            }, stoppingToken);
                        }
                        break;
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                _logger.LogInformation("Successfully processed message from {RoutingKey}", routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {RoutingKey}", routingKey);
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "analytics_service_queue", 
            autoAck: false, 
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ Consumer disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Consumer");
        }
        
        base.Dispose();
    }
}
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskManagement.Contracts.Events;
using TaskService.Application.Contracts.Messaging;

namespace TaskService.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private const string ExchangeName = "task_events";

    public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "rabbitmq",
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false).GetAwaiter().GetResult();

            _logger.LogInformation("RabbitMQ Publisher connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    public void PublishTaskCreated(TaskCreatedEvent taskEvent)
    {
        PublishEvent("task.created", taskEvent);
    }

    public void PublishTaskUpdated(TaskUpdatedEvent taskEvent)
    {
        PublishEvent("task.updated", taskEvent);
    }

    public void PublishTaskCompleted(TaskCompletedEvent taskEvent)
    {
        PublishEvent("task.completed", taskEvent);
    }

    public void PublishTaskDeleted(TaskDeletedEvent taskEvent)
    {
        PublishEvent("task.deleted", taskEvent);
    }

    private void PublishEvent<T>(string routingKey, T eventData)
    {
        try
        {
            var message = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body).GetAwaiter().GetResult();

            _logger.LogInformation("Published event {RoutingKey}: {Message}", routingKey, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {RoutingKey}", routingKey);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ Publisher disposed");
    }
}

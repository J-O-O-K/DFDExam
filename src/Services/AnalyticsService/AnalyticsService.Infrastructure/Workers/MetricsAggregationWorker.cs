using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Application.Features.Metrics.Commands.AggregateMetrics;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Infrastructure.Workers;

public class MetricsAggregationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsAggregationWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public MetricsAggregationWorker(
        IServiceProvider serviceProvider,
        ILogger<MetricsAggregationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics Aggregation Worker started");

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var metricsRepository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();

                // Aggregate metrics for today
                _logger.LogInformation("Aggregating metrics for today...");
                await mediator.Send(new AggregateMetricsCommand { ForDate = DateTime.UtcNow.Date }, stoppingToken);

                // Refresh materialized view
                _logger.LogInformation("Refreshing materialized view...");
                await metricsRepository.RefreshMaterializedViewAsync();
                _logger.LogInformation("Materialized view refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in metrics aggregation");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Metrics Aggregation Worker stopped");
    }
}
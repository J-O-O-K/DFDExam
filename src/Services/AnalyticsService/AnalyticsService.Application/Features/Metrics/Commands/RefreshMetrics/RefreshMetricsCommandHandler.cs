using AnalyticsService.Application.Contracts.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Application.Features.Metrics.Commands.RefreshMetrics;

public class RefreshMetricsCommandHandler : IRequestHandler<RefreshMetricsCommand, Unit>
{
    private readonly IMetricsRepository _repository;
    private readonly ILogger<RefreshMetricsCommandHandler> _logger;

    public RefreshMetricsCommandHandler(
        IMetricsRepository repository,
        ILogger<RefreshMetricsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RefreshMetricsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Manually refreshing materialized view");

        await _repository.RefreshMaterializedViewAsync();

        _logger.LogInformation("Materialized view refreshed successfully");

        return Unit.Value;
    }
}
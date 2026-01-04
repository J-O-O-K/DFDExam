using MediatR;

namespace AnalyticsService.Application.Features.Metrics.Commands.AggregateMetrics;

public class AggregateMetricsCommand : IRequest<bool>
{
    public DateTime? ForDate { get; set; }
}
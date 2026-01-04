using AnalyticsService.Application.DTOs;
using MediatR;

namespace AnalyticsService.Application.Features.Metrics.Queries.GetDashboardMetrics;

public class GetDashboardMetricsQuery : IRequest<IEnumerable<DashboardMetricsDto>>
{
}
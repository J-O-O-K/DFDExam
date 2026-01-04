using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Application.Features.Metrics.Queries.GetDashboardMetrics;
public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, IEnumerable<DashboardMetricsDto>>
{
    private readonly IMetricsRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetDashboardMetricsQueryHandler> _logger;

    public GetDashboardMetricsQueryHandler(
        IMetricsRepository repository,
        IMapper mapper,
        ILogger<GetDashboardMetricsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<DashboardMetricsDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard metrics from PostgreSQL materialized view");

        var metrics = await _repository.GetDashboardMetricsAsync();
        return _mapper.Map<IEnumerable<DashboardMetricsDto>>(metrics);
    }
}
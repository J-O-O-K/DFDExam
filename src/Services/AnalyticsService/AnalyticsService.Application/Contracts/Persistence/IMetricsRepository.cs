using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Application.Contracts.Persistence;

public interface IMetricsRepository
{
    Task<List<TaskMetric>> GetDashboardMetricsAsync();
    Task RefreshMaterializedViewAsync();
    Task UpsertMetricAsync(TaskMetric metric);
}
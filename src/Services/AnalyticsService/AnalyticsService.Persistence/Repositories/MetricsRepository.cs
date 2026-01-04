using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Persistence.Repositories;

public class MetricsRepository : IMetricsRepository
{
    private readonly AnalyticsDbContext _context;

    public MetricsRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskMetric>> GetDashboardMetricsAsync()
    {
        return await _context.TaskMetrics
            .FromSqlRaw("SELECT * FROM mv_daily_task_metrics ORDER BY date DESC")
            .ToListAsync();
    }

    public async Task RefreshMaterializedViewAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("REFRESH MATERIALIZED VIEW CONCURRENTLY mv_daily_task_metrics");
    }

    public async Task UpsertMetricAsync(TaskMetric metric)
    {
        var existing = await _context.TaskMetrics
            .FirstOrDefaultAsync(m => m.Date == metric.Date);

        if (existing != null)
        {
            existing.TotalTasks = metric.TotalTasks;
            existing.CompletedTasks = metric.CompletedTasks;
            existing.OverdueTasks = metric.OverdueTasks;
            existing.AverageCompletionTimeHours = metric.AverageCompletionTimeHours;
            existing.CompletionRate = metric.CompletionRate;
            
            _context.TaskMetrics.Update(existing);
        }
        else
        {
            await _context.TaskMetrics.AddAsync(metric);
        }

        await _context.SaveChangesAsync();
    }
}
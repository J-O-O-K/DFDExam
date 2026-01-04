using AnalyticsService.Domain.Entities;
using AnalyticsService.Persistence.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Persistence.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<TaskMetric> TaskMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TaskMetricConfiguration());
    }
}
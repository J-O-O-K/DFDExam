using AnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalyticsService.Persistence.Data.Configurations;

public class TaskMetricConfiguration : IEntityTypeConfiguration<TaskMetric>
{
    public void Configure(EntityTypeBuilder<TaskMetric> builder)
    {
        builder.ToTable("task_metrics");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(m => m.TotalTasks)
            .HasColumnName("total_tasks")
            .IsRequired();

        builder.Property(m => m.CompletedTasks)
            .HasColumnName("completed_tasks")
            .IsRequired();

        builder.Property(m => m.OverdueTasks)
            .HasColumnName("overdue_tasks")
            .IsRequired();

        builder.Property(m => m.AverageCompletionTimeHours)
            .HasColumnName("average_completion_time_hours")
            .HasPrecision(10, 2);

        builder.Property(m => m.CompletionRate)
            .HasColumnName("completion_rate")
            .HasPrecision(5, 2);

        builder.HasIndex(m => m.Date);
    }
}
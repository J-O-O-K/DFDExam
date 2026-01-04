using Microsoft.EntityFrameworkCore;
using TaskService.Domain.Entities;
using TaskService.Persistence.Data.Configurations;

namespace TaskService.Persistence.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<TaskAuditLog> TaskAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new TaskAuditLogConfiguration());
    }
}

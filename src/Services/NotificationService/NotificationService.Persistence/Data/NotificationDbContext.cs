using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Persistence.Data.Configurations;

namespace NotificationService.Persistence.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
}
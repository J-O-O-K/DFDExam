using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskService.Domain.Entities;

namespace TaskService.Persistence.Data.Configurations;

public class TaskAuditLogConfiguration : IEntityTypeConfiguration<TaskAuditLog>
{
    public void Configure(EntityTypeBuilder<TaskAuditLog> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TaskId)
            .IsRequired();

        builder.Property(t => t.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.ChangedAt)
            .IsRequired();

        builder.Property(t => t.OldValues)
            .HasColumnType("text");

        builder.Property(t => t.NewValues)
            .HasColumnType("text");

        builder.HasIndex(t => t.TaskId);
        builder.HasIndex(t => t.ChangedAt);
    }
}

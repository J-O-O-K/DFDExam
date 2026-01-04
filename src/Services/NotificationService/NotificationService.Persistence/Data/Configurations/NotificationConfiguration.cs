using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Persistence.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<NotificationType>(v));

        builder.Property(n => n.IsRead)
            .HasColumnName("is_read")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.RelatedTaskId)
            .HasColumnName("related_task_id")
            .IsRequired();

        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.RelatedTaskId);
    }
}
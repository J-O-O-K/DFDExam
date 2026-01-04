using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;
public class Notification
{
    public int Id { get; set; }
    public string? Message { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public int RelatedTaskId { get; set; }
}
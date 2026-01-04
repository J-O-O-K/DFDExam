namespace NotificationService.Application.DTOs;
public class CreateNotificationDto
{
    public string? Message { get; set; }
    public string? Type { get; set; }
    public int RelatedTaskId { get; set; }
}
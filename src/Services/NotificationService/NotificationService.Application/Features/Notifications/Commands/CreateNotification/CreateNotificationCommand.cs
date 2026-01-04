using MediatR;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Features.Notifications.Commands.CreateNotification;
public class CreateNotificationCommand : IRequest<NotificationDto>
{
    public string? Message { get; set; }
    public string? Type { get; set; }
    public int RelatedTaskId { get; set; }
}
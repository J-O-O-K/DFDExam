using MediatR;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Features.Notifications.Queries.GetNotificationById;

public class GetNotificationByIdQuery : IRequest<NotificationDto?>
{
    public int Id { get; set; }
}
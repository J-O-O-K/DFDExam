using MediatR;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Features.Notifications.Queries.GetRecentNotifications;

public class GetRecentNotificationsQuery : IRequest<IEnumerable<NotificationDto>>
{
    public int Count { get; set; } = 50;
}
using MediatR;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommand : IRequest<NotificationDto>
{
    public int Id { get; set; }
}
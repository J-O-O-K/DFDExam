using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Features.Notifications.Commands.MarkAsRead;
using NotificationService.Application.Features.Notifications.Queries.GetNotificationById;
using NotificationService.Application.Features.Notifications.Queries.GetRecentNotifications;
using NotificationService.Application.Features.Notifications.Queries.GetUnreadCount;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetRecentNotifications([FromQuery] int count = 50)
    {
        _logger.LogInformation("Getting {Count} recent notifications", count);
        var notifications = await _mediator.Send(new GetRecentNotificationsQuery { Count = count });
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> GetNotificationById(int id)
    {
        _logger.LogInformation("Getting notification with ID: {NotificationId}", id);
        var notification = await _mediator.Send(new GetNotificationByIdQuery { Id = id });

        if (notification == null)
        {
            _logger.LogWarning("Notification with ID {NotificationId} not found", id);
            return NotFound(new { Message = $"Notification with ID {id} not found" });
        }

        return Ok(notification);
    }

    [HttpPost("{id}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(int id)
    {
        _logger.LogInformation("Marking notification {NotificationId} as read", id);

        try
        {
            var notification = await _mediator.Send(new MarkAsReadCommand { Id = id });
            return Ok(notification);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Notification with ID {NotificationId} not found", id);
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        _logger.LogInformation("Getting unread notification count");
        var count = await _mediator.Send(new GetUnreadCountQuery());
        return Ok(count);
    }
}
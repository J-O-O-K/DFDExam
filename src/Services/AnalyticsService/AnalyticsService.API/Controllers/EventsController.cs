using AnalyticsService.Application.DTOs;
using AnalyticsService.Application.Features.Events.Queries.GetEventsByTaskId;
using AnalyticsService.Application.Features.Events.Queries.GetRecentEvents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.API.Controllers;

[ApiController]
[Route("api/analytics/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IMediator mediator, ILogger<EventsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskEventDto>>> GetRecentEvents([FromQuery] int limit = 100)
    {
        _logger.LogInformation("Getting {Limit} recent task events from MongoDB", limit);
        var events = await _mediator.Send(new GetRecentEventsQuery { Limit = limit });
        return Ok(events);
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(IEnumerable<TaskEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskEventDto>>> GetEventsByTaskId(int taskId)
    {
        _logger.LogInformation("Getting events for TaskId {TaskId} from MongoDB", taskId);
        var events = await _mediator.Send(new GetEventsByTaskIdQuery { TaskId = taskId });
        return Ok(events);
    }
}
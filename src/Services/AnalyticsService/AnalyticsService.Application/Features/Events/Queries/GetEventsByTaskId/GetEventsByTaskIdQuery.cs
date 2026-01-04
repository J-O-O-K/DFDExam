using AnalyticsService.Application.DTOs;
using MediatR;

namespace AnalyticsService.Application.Features.Events.Queries.GetEventsByTaskId;

public class GetEventsByTaskIdQuery : IRequest<IEnumerable<TaskEventDto>>
{
    public int TaskId { get; set; }
}
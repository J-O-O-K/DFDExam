using AnalyticsService.Application.DTOs;
using MediatR;

namespace AnalyticsService.Application.Features.Events.Queries.GetRecentEvents;

public class GetRecentEventsQuery : IRequest<IEnumerable<TaskEventDto>>
{
    public int Limit { get; set; } = 100;
}
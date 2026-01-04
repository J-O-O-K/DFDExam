using AnalyticsService.Application.DTOs;
using MediatR;

namespace AnalyticsService.Application.Features.Events.Commands.LogTaskEvent;
public class LogTaskEventCommand : IRequest<TaskEventDto>
{
    public string EventType { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
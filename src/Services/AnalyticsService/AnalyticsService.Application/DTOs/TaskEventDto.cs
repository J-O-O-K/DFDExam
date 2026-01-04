using MongoDB.Bson;

namespace AnalyticsService.Application.DTOs;
public class TaskEventDto
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
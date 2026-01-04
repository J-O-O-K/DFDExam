using AnalyticsService.Domain.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyticsService.Domain.Entities;

[BsonCollection("taskevents")]
public class TaskEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventType")]
    public string? EventType { get; set; }

    [BsonElement("taskId")]
    public int TaskId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonExtraElements]
    public BsonDocument Metadata { get; set; } = new();
}
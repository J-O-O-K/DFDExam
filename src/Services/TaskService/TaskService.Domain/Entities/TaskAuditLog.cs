namespace TaskService.Domain.Entities;
public class TaskAuditLog
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string? Action { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}

namespace TaskManagement.Contracts.Events;
public class TaskUpdatedEvent
{
    public int TaskId { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}

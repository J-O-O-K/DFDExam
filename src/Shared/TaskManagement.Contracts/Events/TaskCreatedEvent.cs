namespace TaskManagement.Contracts.Events;
public class TaskCreatedEvent
{
    public int TaskId { get; set; }
    public string? Title { get; set; }
    public string? Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}

namespace TaskManagement.Contracts.Events;
public class TaskCompletedEvent
{
    public int TaskId { get; set; }
    public DateTime CompletedAt { get; set; }
}

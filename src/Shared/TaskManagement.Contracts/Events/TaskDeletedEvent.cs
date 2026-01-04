namespace TaskManagement.Contracts.Events;
public class TaskDeletedEvent
{
    public int TaskId { get; set; }
    public DateTime DeletedAt { get; set; }
}

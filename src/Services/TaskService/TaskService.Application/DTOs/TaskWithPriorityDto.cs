namespace TaskService.Application.DTOs;

public class TaskWithPriorityDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public int CalculatedPriority { get; set; }
}

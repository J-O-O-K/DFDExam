using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommand : IRequest<TaskDto>
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

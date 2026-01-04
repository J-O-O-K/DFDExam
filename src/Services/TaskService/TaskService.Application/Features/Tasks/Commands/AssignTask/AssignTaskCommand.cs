using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Commands.AssignTask;

public class AssignTaskCommand : IRequest<TaskDto>
{
    public int Id { get; set; }
    public string? AssignedTo { get; set; }
}

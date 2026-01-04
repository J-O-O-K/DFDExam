using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Commands.CompleteTask;

public class CompleteTaskCommand : IRequest<TaskDto>
{
    public int Id { get; set; }
}

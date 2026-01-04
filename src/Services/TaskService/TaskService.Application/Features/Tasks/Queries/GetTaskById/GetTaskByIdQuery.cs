using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetTaskById;

public class GetTaskByIdQuery : IRequest<TaskDto?>
{
    public int Id { get; set; }
}

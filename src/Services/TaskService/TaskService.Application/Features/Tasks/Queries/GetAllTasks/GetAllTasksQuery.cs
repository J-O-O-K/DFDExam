using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetAllTasks;

public class GetAllTasksQuery : IRequest<IEnumerable<TaskDto>>
{
}

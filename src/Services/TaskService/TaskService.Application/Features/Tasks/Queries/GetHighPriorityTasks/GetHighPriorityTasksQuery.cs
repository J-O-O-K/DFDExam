using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetHighPriorityTasks;

public class GetHighPriorityTasksQuery : IRequest<IEnumerable<TaskWithPriorityDto>>
{
}

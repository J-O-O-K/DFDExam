using MediatR;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetOverdueTasks
{

    public class GetOverdueTasksQuery : IRequest<IEnumerable<TaskOverdueDto>>
    {
    }
}

using MediatR;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetHighPriorityTasks;

public class GetHighPriorityTasksQueryHandler : IRequestHandler<GetHighPriorityTasksQuery, IEnumerable<TaskWithPriorityDto>>
{
    private readonly ITaskRepository _repository;

    public GetHighPriorityTasksQueryHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaskWithPriorityDto>> Handle(GetHighPriorityTasksQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetHighPriorityTasksAsync();
    }
}

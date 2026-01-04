using MediatR;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetOverdueTasks
{

    public class GetOverdueTasksQueryHandler : IRequestHandler<GetOverdueTasksQuery, IEnumerable<TaskOverdueDto>>
    {
        private readonly ITaskRepository _repository;

        public GetOverdueTasksQueryHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TaskOverdueDto>> Handle(GetOverdueTasksQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetOverdueTasksAsync();
        }
    }
}

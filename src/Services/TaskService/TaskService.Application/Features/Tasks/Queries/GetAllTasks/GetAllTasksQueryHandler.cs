using AutoMapper;
using MediatR;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Queries.GetAllTasks;

public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, IEnumerable<TaskDto>>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;

    public GetAllTasksQueryHandler(ITaskRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }
}

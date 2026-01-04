using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Application.Features.Events.Queries.GetEventsByTaskId;
public class GetEventsByTaskIdQueryHandler : IRequestHandler<GetEventsByTaskIdQuery, IEnumerable<TaskEventDto>>
{
    private readonly ITaskEventRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEventsByTaskIdQueryHandler> _logger;

    public GetEventsByTaskIdQueryHandler(
        ITaskEventRepository repository,
        IMapper mapper,
        ILogger<GetEventsByTaskIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskEventDto>> Handle(GetEventsByTaskIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching events for TaskId {TaskId} from MongoDB", request.TaskId);

        var events = await _repository.GetByTaskIdAsync(request.TaskId);
        return _mapper.Map<IEnumerable<TaskEventDto>>(events);
    }
}
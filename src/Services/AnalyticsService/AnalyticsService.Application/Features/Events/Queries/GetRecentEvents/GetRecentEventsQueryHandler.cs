using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Application.Features.Events.Queries.GetRecentEvents;

public class GetRecentEventsQueryHandler : IRequestHandler<GetRecentEventsQuery, IEnumerable<TaskEventDto>>
{
    private readonly ITaskEventRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetRecentEventsQueryHandler> _logger;

    public GetRecentEventsQueryHandler(
        ITaskEventRepository repository,
        IMapper mapper,
        ILogger<GetRecentEventsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskEventDto>> Handle(GetRecentEventsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching {Limit} recent events from MongoDB", request.Limit);

        var events = await _repository.GetRecentAsync(request.Limit);
        return _mapper.Map<IEnumerable<TaskEventDto>>(events);
    }
}
using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Application.DTOs;
using AnalyticsService.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace AnalyticsService.Application.Features.Events.Commands.LogTaskEvent;
public class LogTaskEventCommandHandler : IRequestHandler<LogTaskEventCommand, TaskEventDto>
{
    private readonly ITaskEventRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<LogTaskEventCommandHandler> _logger;

    public LogTaskEventCommandHandler(
        ITaskEventRepository repository,
        IMapper mapper,
        ILogger<LogTaskEventCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TaskEventDto> Handle(LogTaskEventCommand request, CancellationToken cancellationToken)
    {
        var taskEvent = new TaskEvent
        {
            EventType = request.EventType,
            TaskId = request.TaskId,
            Timestamp = DateTime.UtcNow,
            Metadata = new BsonDocument(request.Metadata.Select(kvp =>
                new BsonElement(kvp.Key, BsonValue.Create(kvp.Value))))
        };

        await _repository.CreateAsync(taskEvent);

        _logger.LogInformation("Task event logged: {EventType} for TaskId {TaskId}",
            request.EventType, request.TaskId);

        return _mapper.Map<TaskEventDto>(taskEvent);
    }
}
using AutoMapper;
using MediatR;
using TaskManagement.Contracts.Events;
using TaskService.Application.Contracts.Messaging;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;

namespace TaskService.Application.Features.Tasks.Commands.AssignTask;

public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMessagePublisher _messagePublisher;

    public AssignTaskCommandHandler(
        ITaskRepository repository,
        IMapper mapper,
        IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _messagePublisher = messagePublisher;
    }

    public async Task<TaskDto> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _repository.GetByIdAsync(request.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found");
        }

        existingTask.AssignedTo = request.AssignedTo;
        existingTask.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _repository.UpdateAsync(existingTask);

        // Publish event
        _messagePublisher.PublishTaskUpdated(new TaskUpdatedEvent
        {
            TaskId = updatedTask.Id,
            UpdatedAt = updatedTask.UpdatedAt.Value
        });

        return _mapper.Map<TaskDto>(updatedTask);
    }
}

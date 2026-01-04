using AutoMapper;
using MediatR;
using TaskManagement.Contracts.Events;
using TaskService.Application.Contracts.Messaging;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;
using TaskService.Domain.Enums;
using TaskStatus = TaskService.Domain.Enums.TaskStatus;

namespace TaskService.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMessagePublisher _messagePublisher;

    public UpdateTaskCommandHandler(
        ITaskRepository repository,
        IMapper mapper,
        IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _messagePublisher = messagePublisher;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _repository.GetByIdAsync(request.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
            existingTask.Title = request.Title;

        if (!string.IsNullOrWhiteSpace(request.Description))
            existingTask.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.Status))
            existingTask.Status = Enum.Parse<TaskStatus>(request.Status, true);

        if (!string.IsNullOrWhiteSpace(request.Priority))
            existingTask.Priority = Enum.Parse<TaskPriority>(request.Priority, true);

        if (request.DueDate.HasValue)
            existingTask.DueDate = request.DueDate.Value;

        existingTask.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _repository.UpdateAsync(existingTask);

        // Publish event
        _messagePublisher.PublishTaskUpdated(new TaskUpdatedEvent
        {
            TaskId = updatedTask.Id,
            Title = request.Title,
            Status = request.Status,
            UpdatedAt = updatedTask.UpdatedAt.Value
        });

        return _mapper.Map<TaskDto>(updatedTask);
    }
}

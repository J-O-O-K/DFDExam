using AutoMapper;
using MediatR;
using TaskManagement.Contracts.Events;
using TaskService.Application.Contracts.Messaging;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;
using TaskService.Domain.Enums;
using TaskStatus = TaskService.Domain.Enums.TaskStatus;

namespace TaskService.Application.Features.Tasks.Commands.CompleteTask;

public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMessagePublisher _messagePublisher;

    public CompleteTaskCommandHandler(
        ITaskRepository repository,
        IMapper mapper,
        IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _messagePublisher = messagePublisher;
    }

    public async Task<TaskDto> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _repository.GetByIdAsync(request.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found");
        }

        existingTask.Status = TaskStatus.Completed;
        existingTask.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _repository.UpdateAsync(existingTask);

        // Publish event
        _messagePublisher.PublishTaskCompleted(new TaskCompletedEvent
        {
            TaskId = updatedTask.Id,
            CompletedAt = updatedTask.UpdatedAt.Value
        });

        return _mapper.Map<TaskDto>(updatedTask);
    }
}

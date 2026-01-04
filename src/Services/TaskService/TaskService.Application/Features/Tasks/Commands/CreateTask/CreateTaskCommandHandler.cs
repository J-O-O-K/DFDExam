using AutoMapper;
using MediatR;
using TaskManagement.Contracts.Events;
using TaskService.Application.Contracts.Messaging;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;
using TaskService.Domain.Enums;
using TaskStatus = TaskService.Domain.Enums.TaskStatus;

namespace TaskService.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMessagePublisher _messagePublisher;

    public CreateTaskCommandHandler(
        ITaskRepository repository,
        IMapper mapper,
        IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _messagePublisher = messagePublisher;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskEntity = new TaskEntity
        {
            Title = request.Title,
            Description = request.Description,
            Priority = Enum.Parse<TaskPriority>(request.Priority, true),
            DueDate = request.DueDate,
            Status = TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createdTask = await _repository.CreateAsync(taskEntity);

        // Publish event
        _messagePublisher.PublishTaskCreated(new TaskCreatedEvent
        {
            TaskId = createdTask.Id,
            Title = createdTask.Title,
            Priority = createdTask.Priority.ToString(),
            CreatedAt = createdTask.CreatedAt
        });

        return _mapper.Map<TaskDto>(createdTask);
    }
}

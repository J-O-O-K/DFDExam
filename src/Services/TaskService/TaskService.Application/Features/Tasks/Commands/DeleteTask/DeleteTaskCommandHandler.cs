using MediatR;
using TaskManagement.Contracts.Events;
using TaskService.Application.Contracts.Messaging;
using TaskService.Application.Contracts.Persistence;

namespace TaskService.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly ITaskRepository _repository;
    private readonly IMessagePublisher _messagePublisher;

    public DeleteTaskCommandHandler(
        ITaskRepository repository,
        IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var existingTask = await _repository.GetByIdAsync(request.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {request.Id} not found");
        }

        await _repository.DeleteAsync(existingTask);

        // Publish event
        _messagePublisher.PublishTaskDeleted(new TaskDeletedEvent
        {
            TaskId = request.Id,
            DeletedAt = DateTime.UtcNow
        });

        return Unit.Value;
    }
}

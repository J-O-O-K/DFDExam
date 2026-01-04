using TaskManagement.Contracts.Events;

namespace TaskService.Application.Contracts.Messaging;

public interface IMessagePublisher
{
    void PublishTaskCreated(TaskCreatedEvent taskEvent);
    void PublishTaskUpdated(TaskUpdatedEvent taskEvent);
    void PublishTaskCompleted(TaskCompletedEvent taskEvent);
    void PublishTaskDeleted(TaskDeletedEvent taskEvent);
}

using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Application.Contracts.Persistence;

public interface ITaskEventRepository
{
    Task<TaskEvent> CreateAsync(TaskEvent taskEvent);
    Task<IEnumerable<TaskEvent>> GetByTaskIdAsync(int taskId);
    Task<IEnumerable<TaskEvent>> GetEventsByTaskIdAsync(int taskId);
    Task<IEnumerable<TaskEvent>> GetEventsByDateAsync(DateTime date);
    Task<IEnumerable<TaskEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TaskEvent>> GetRecentAsync(int limit);
}
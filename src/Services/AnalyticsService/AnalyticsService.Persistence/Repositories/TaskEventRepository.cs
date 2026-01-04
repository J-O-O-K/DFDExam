using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Persistence.Data;
using MongoDB.Driver;

namespace AnalyticsService.Persistence.Repositories;

public class TaskEventRepository : ITaskEventRepository
{
    private readonly IMongoCollection<TaskEvent> _collection;

    public TaskEventRepository(MongoDbService mongoDbService)
    {
        _collection = mongoDbService.GetCollection<TaskEvent>();
    }

    public async Task<TaskEvent> CreateAsync(TaskEvent taskEvent)
    {
        await _collection.InsertOneAsync(taskEvent);
        return taskEvent;
    }

    public async Task<IEnumerable<TaskEvent>> GetByTaskIdAsync(int taskId)
    {
        return await _collection
            .Find(e => e.TaskId == taskId)
            .SortByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEvent>> GetEventsByTaskIdAsync(int taskId)
    {
        return await GetByTaskIdAsync(taskId);
    }

    public async Task<IEnumerable<TaskEvent>> GetEventsByDateAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        return await _collection
            .Find(e => e.Timestamp >= startDate && e.Timestamp < endDate)
            .SortByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _collection
            .Find(e => e.Timestamp >= startDate && e.Timestamp < endDate)
            .SortByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEvent>> GetRecentAsync(int limit)
    {
        return await _collection
            .Find(_ => true)
            .SortByDescending(e => e.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
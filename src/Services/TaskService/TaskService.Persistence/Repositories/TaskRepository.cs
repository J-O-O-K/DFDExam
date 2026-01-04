using Dapper;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Contracts.Persistence;
using TaskService.Application.DTOs;
using TaskService.Domain.Entities;
using TaskService.Persistence.Data;

namespace TaskService.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;

    public TaskRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<TaskEntity> CreateAsync(TaskEntity task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskEntity?> GetByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<IEnumerable<TaskEntity>> GetAllAsync()
    {
        return await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskWithPriorityDto>> GetHighPriorityTasksAsync()
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        var result = await connection.QueryAsync<TaskWithPriorityDto>(
            "SELECT * FROM sp_GetHighPriorityTasks()");

        return result;
    }

    public async Task<IEnumerable<TaskOverdueDto>> GetOverdueTasksAsync()
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        var result = await connection.QueryAsync<TaskOverdueDto>(
            "SELECT * FROM sp_GetOverdueTasks()");

        return result;
    }

    public async Task<TaskEntity> UpdateAsync(TaskEntity task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task DeleteAsync(TaskEntity task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == id);
    }
}
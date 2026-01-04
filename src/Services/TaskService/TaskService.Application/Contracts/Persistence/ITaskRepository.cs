using TaskService.Application.DTOs;
using TaskService.Domain.Entities;

namespace TaskService.Application.Contracts.Persistence;

public interface ITaskRepository
{
    Task<TaskEntity> CreateAsync(TaskEntity task);
    Task<TaskEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TaskEntity>> GetAllAsync();
    Task<IEnumerable<TaskWithPriorityDto>> GetHighPriorityTasksAsync();
    Task<IEnumerable<TaskOverdueDto>> GetOverdueTasksAsync();
    Task<TaskEntity> UpdateAsync(TaskEntity task);
    Task DeleteAsync(TaskEntity task);
    Task<bool> ExistsAsync(int id);
}

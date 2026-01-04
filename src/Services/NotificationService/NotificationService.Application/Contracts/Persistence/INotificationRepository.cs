using NotificationService.Domain.Entities;

namespace NotificationService.Application.Contracts.Persistence;
public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification?> GetByIdAsync(int id);
    Task<IEnumerable<Notification>> GetRecentAsync(int count = 50);
    Task<int> GetUnreadCountAsync();
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> ExistsAsync(int id);
}
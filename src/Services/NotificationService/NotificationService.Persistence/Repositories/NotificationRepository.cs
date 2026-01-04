using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Contracts.Persistence;
using NotificationService.Domain.Entities;
using NotificationService.Persistence.Data;

namespace NotificationService.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<IEnumerable<Notification>> GetRecentAsync(int count = 50)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.Notifications
            .Where(n => !n.IsRead)
            .CountAsync();
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Notifications.AnyAsync(n => n.Id == id);
    }
}
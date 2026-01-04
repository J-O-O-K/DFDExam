using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.Caching;
using NotificationService.Application.Contracts.Persistence;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, NotificationDto>
{
    private readonly INotificationRepository _repository;
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly ILogger<MarkAsReadCommandHandler> _logger;

    public MarkAsReadCommandHandler(
        INotificationRepository repository,
        IRedisService redisService,
        IMapper mapper,
        ILogger<MarkAsReadCommandHandler> logger)
    {
        _repository = repository;
        _redisService = redisService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NotificationDto> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(request.Id);
        if (notification == null)
        {
            throw new KeyNotFoundException($"Notification with ID {request.Id} not found");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _repository.UpdateAsync(notification);

            // Update individual cache
            var individualCacheKey = $"notification:{notification.Id}";
            await _redisService.SetCachedAsync(individualCacheKey, notification, TimeSpan.FromHours(24));

            var listCacheKey = "notifications:recent:50";
            var cachedList = await _redisService.GetCachedAsync<List<NotificationDto>>(listCacheKey);
            if (cachedList != null)
            {
                var cachedNotification = cachedList.FirstOrDefault(n => n.Id == request.Id);
                if (cachedNotification != null)
                {
                    cachedNotification.IsRead = true;
                    await _redisService.SetCachedAsync(listCacheKey, cachedList, TimeSpan.FromMinutes(1));
                    _logger.LogInformation("Updated notification {NotificationId} in cached list", notification.Id);
                }
            }

            // Decrement count
            await _redisService.DecrementAsync("notifications:unread_count");

            _logger.LogInformation("Notification {NotificationId} marked as read", notification.Id);
        }

        return _mapper.Map<NotificationDto>(notification);
    }
}
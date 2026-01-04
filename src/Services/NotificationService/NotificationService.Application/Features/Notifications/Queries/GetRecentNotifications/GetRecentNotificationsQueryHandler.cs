using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.Caching;
using NotificationService.Application.Contracts.Persistence;
using NotificationService.Application.DTOs;
using NotificationService.Application.Features.Notifications.Queries.GetRecentNotifications;

namespace NotificationService.Application.Queries.GetRecentNotifications;

public class GetRecentNotificationsQueryHandler : IRequestHandler<GetRecentNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationRepository _repository;
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetRecentNotificationsQueryHandler> _logger;

    public GetRecentNotificationsQueryHandler(
        INotificationRepository repository,
        IRedisService redisService,
        IMapper mapper,
        ILogger<GetRecentNotificationsQueryHandler> logger)
    {
        _repository = repository;
        _redisService = redisService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetRecentNotificationsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"notifications:recent:{request.Count}";

        // Try to get from Redis cache first
        var cachedNotifications = await _redisService.GetCachedAsync<IEnumerable<NotificationDto>>(cacheKey);
        if (cachedNotifications != null && cachedNotifications.Any())
        {
            _logger.LogInformation("Retrieved {Count} notifications from Redis cache", cachedNotifications.Count());
            return cachedNotifications;
        }

        // Fallback to PostgreSQL
        _logger.LogInformation("Cache miss - fetching from PostgreSQL");
        var notifications = await _repository.GetRecentAsync(request.Count);
        var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

        // Avoid caching empty arrays
        if (notificationDtos.Any())
        {
            await _redisService.SetCachedAsync(cacheKey, notificationDtos, TimeSpan.FromMinutes(1));
        }

        return notificationDtos;
    }
}
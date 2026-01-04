using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.Caching;
using NotificationService.Application.Contracts.Persistence;

namespace NotificationService.Application.Features.Notifications.Queries.GetUnreadCount;
public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _repository;
    private readonly IRedisService _redisService;
    private readonly ILogger<GetUnreadCountQueryHandler> _logger;

    public GetUnreadCountQueryHandler(
        INotificationRepository repository,
        IRedisService redisService,
        ILogger<GetUnreadCountQueryHandler> logger)
    {
        _repository = repository;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = "notifications:unread_count";

        // Try to get from Redis
        var cachedCount = await _redisService.GetCachedAsync<int?>(cacheKey);
        if (cachedCount.HasValue)
        {
            _logger.LogInformation("Retrieved unread count from Redis: {Count}", cachedCount.Value);
            return cachedCount.Value;
        }

        // Fallback to PostgreSQL
        _logger.LogInformation("Cache miss - fetching unread count from PostgreSQL");
        var count = await _repository.GetUnreadCountAsync();

        await _redisService.SetCachedAsync(cacheKey, count, TimeSpan.FromMinutes(1));

        return count;
    }
}
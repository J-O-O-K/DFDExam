using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.Caching;
using NotificationService.Application.Contracts.Persistence;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Features.Notifications.Queries.GetNotificationById;

public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, NotificationDto?>
{
    private readonly INotificationRepository _repository;
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetNotificationByIdQueryHandler> _logger;

    public GetNotificationByIdQueryHandler(
        INotificationRepository repository,
        IRedisService redisService,
        IMapper mapper,
        ILogger<GetNotificationByIdQueryHandler> logger)
    {
        _repository = repository;
        _redisService = redisService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NotificationDto?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"notification:{request.Id}";

        // Try Redis cache
        var cachedNotification = await _redisService.GetCachedAsync<Notification>(cacheKey);
        if (cachedNotification != null)
        {
            _logger.LogInformation("Retrieved notification {NotificationId} from Redis cache", request.Id);
            return _mapper.Map<NotificationDto>(cachedNotification);
        }

        // Fallback to PostgreSQL
        _logger.LogInformation("Cache miss - fetching notification {NotificationId} from PostgreSQL", request.Id);
        var notification = await _repository.GetByIdAsync(request.Id);

        if (notification != null)
        {
            // Cache 24 hours
            await _redisService.SetCachedAsync(cacheKey, notification, TimeSpan.FromHours(24));
        }

        return notification == null ? null : _mapper.Map<NotificationDto>(notification);
    }
}
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.Caching;
using NotificationService.Application.Contracts.Persistence;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Application.Features.Notifications.Commands.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _repository;
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateNotificationCommandHandler> _logger;

    public CreateNotificationCommandHandler(
        INotificationRepository repository,
        IRedisService redisService,
        IMapper mapper,
        ILogger<CreateNotificationCommandHandler> logger)
    {
        _repository = repository;
        _redisService = redisService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Message = request.Message,
            Type = Enum.Parse<NotificationType>(request.Type, true),
            RelatedTaskId = request.RelatedTaskId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        var createdNotification = await _repository.CreateAsync(notification);

        var cacheKey = $"notification:{createdNotification.Id}";
        await _redisService.SetCachedAsync(cacheKey, createdNotification, TimeSpan.FromHours(24));

        await _redisService.IncrementAsync("notifications:unread_count");

        var notificationDto = _mapper.Map<NotificationDto>(createdNotification);
        var message = JsonSerializer.Serialize(notificationDto);
        await _redisService.PublishAsync("notifications", message);

        _logger.LogInformation("Notification created and published: ID {NotificationId}", createdNotification.Id);

        return notificationDto;
    }
}
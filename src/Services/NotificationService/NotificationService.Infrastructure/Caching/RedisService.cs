using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Contracts.Caching;
using StackExchange.Redis;

namespace NotificationService.Infrastructure.Caching;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetCachedAsync<T>(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from Redis cache: {Key}", key);
            return default;
        }
    }

    public async Task SetCachedAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            if (expiry.HasValue)
            {
                await _db.StringSetAsync(key, serialized, expiry.Value);
            }
            else
            {
                await _db.StringSetAsync(key, serialized);
            }
            _logger.LogDebug("Cached value set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting Redis cache: {Key}", key);
        }
    }

    public async Task<bool> DeleteCachedAsync(string key)
    {
        try
        {
            return await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting from Redis cache: {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key)
    {
        try
        {
            return await _db.StringIncrementAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing Redis key: {Key}", key);
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key)
    {
        try
        {
            return await _db.StringDecrementAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing Redis key: {Key}", key);
            return 0;
        }
    }

    public async Task PublishAsync(string channel, string message)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(channel, message);
            _logger.LogInformation("Published message to channel {Channel}", channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing to Redis channel: {Channel}", channel);
        }
    }

    public void Subscribe(string channel, Action<string> handler)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            subscriber.Subscribe(channel, (ch, message) =>
            {
                if (!message.IsNullOrEmpty)
                {
                    handler(message!);
                }
            });
            _logger.LogInformation("Subscribed to Redis channel: {Channel}", channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to Redis channel: {Channel}", channel);
        }
    }
}
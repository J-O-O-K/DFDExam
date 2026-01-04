namespace NotificationService.Application.Contracts.Caching;

public interface IRedisService
{
    Task<T?> GetCachedAsync<T>(string key);
    Task SetCachedAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> DeleteCachedAsync(string key);
    Task<long> IncrementAsync(string key);
    Task<long> DecrementAsync(string key);
    Task PublishAsync(string channel, string message);
    void Subscribe(string channel, Action<string> handler);
}
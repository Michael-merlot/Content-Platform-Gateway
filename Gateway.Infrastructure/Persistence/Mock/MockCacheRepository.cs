using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Persistence.Mock
{
    /// <summary>
    /// Реализация кэш-репозитория, использующая in-memory словарь вместо Redis
    /// </summary>
    public class MockCacheRepository : ICacheRepository
    {
        private readonly ILogger<MockCacheRepository> _logger;
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();

        public MockCacheRepository(ILogger<MockCacheRepository> logger)
        {
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _logger.LogInformation("MOCK CACHE: Getting value for key {Key}", key);

            if (_cache.TryGetValue(key, out var item) &&
                (item.ExpiryTime == null || item.ExpiryTime > DateTimeOffset.UtcNow))
            {
                _logger.LogInformation("MOCK CACHE: HIT for key {Key}", key);
                return Task.FromResult(item.Value is T value ? value : default(T?));
            }

            _logger.LogInformation("MOCK CACHE: MISS for key {Key}", key);
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            _logger.LogInformation("MOCK CACHE: Setting value for key {Key}", key);

            var expiryTime = expiry.HasValue ? DateTimeOffset.UtcNow.Add(expiry.Value) : (DateTimeOffset?)null;
            _cache[key] = new CacheItem(value!, expiryTime);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _logger.LogInformation("MOCK CACHE: Removing key {Key}", key);
            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _cache.TryGetValue(key, out var item) &&
                        (item.ExpiryTime == null || item.ExpiryTime > DateTimeOffset.UtcNow);

            _logger.LogInformation("MOCK CACHE: Key {Key} exists: {Exists}", key, exists);
            return Task.FromResult(exists);
        }

        private class CacheItem
        {
            public CacheItem(object value, DateTimeOffset? expiryTime)
            {
                Value = value;
                ExpiryTime = expiryTime;
            }

            public object Value { get; }
            public DateTimeOffset? ExpiryTime { get; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Gateway.Core.Interfaces.Persistence;

namespace Gateway.Infrastructure.Persistence.Memory
{
    /// <summary>
    /// In-memory реализация кэша для часто запрашиваемых данных.
    /// </summary>
    public class MemoryCacheRepository : IMemoryCacheRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheRepository> _logger;

        public MemoryCacheRepository(IMemoryCache memoryCache, ILogger<MemoryCacheRepository> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (_memoryCache.TryGetValue(key, out var value) && value is T typed)
            {
                _logger.LogDebug("Cache(Memory) hit for key {Key}", key);
                return Task.FromResult<T?>(typed);
            }

            _logger.LogDebug("Cache(Memory) miss for key {Key}", key);
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);
            else
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // значение по умолчанию

            _memoryCache.Set(key, value, options);

            _logger.LogDebug("Set key {Key} in cache(Memory) with expiration {Expiration}",
                key, expiration?.ToString() ?? "30 minutes");

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Removed key {Key} from cache(Memory)", key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            _logger.LogDebug("Exists check in cache(Memory) for key {Key}: {Result}", key, exists);
            return Task.FromResult(exists);
        }
    }
}

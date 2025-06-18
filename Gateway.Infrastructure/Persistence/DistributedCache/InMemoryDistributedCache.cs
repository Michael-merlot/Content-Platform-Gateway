using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Persistence.DistributedCache
{
    public class InMemoryDistributedCache : IDistributedCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemoryDistributedCache> _logger;
        private readonly Dictionary<string, bool> _keysTracker = new();

        public InMemoryDistributedCache(IMemoryCache memoryCache, ILogger<InMemoryDistributedCache> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _logger.LogInformation("Using in-memory cache instead of Redis");
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_keysTracker.ContainsKey(key));
        }

        public Task<T> GetAsync<T>(string key) where T : class
        {
            if (_memoryCache.TryGetValue(key, out string data) && !string.IsNullOrEmpty(data))
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(data);
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing data from cache for key {Key}", key);
                }
            }

            return Task.FromResult<T>(null);
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _keysTracker.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);
            else
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            var data = JsonSerializer.Serialize(value);
            _memoryCache.Set(key, data, options);
            _keysTracker[key] = true;

            return Task.CompletedTask;
        }
    }
}

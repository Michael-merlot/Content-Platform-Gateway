using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Persistence.DistributedCache
{
    public class RedisDistributedCache : IDistributedCacheService
    {
        private readonly IDistributedCache _distributedCache;

        public RedisDistributedCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            var data = await _distributedCache.GetStringAsync(key);

            if (string.IsNullOrEmpty(data))
                return null;

            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null) where T : class
        {
            var options = new DistributedCacheEntryOptions();

            if (absoluteExpiration.HasValue)
                options.SetAbsoluteExpiration(absoluteExpiration.Value);
            else
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // значение по умолчанию

            var data = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, data, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}

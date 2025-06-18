using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Persistence.DistributedCache
{

    public class RedisDistributedCache : IDistributedCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisDistributedCache> _logger;

        public RedisDistributedCache(IDistributedCache distributedCache, ILogger<RedisDistributedCache> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var data = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(data);
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(data))
                {
                    _logger.LogDebug("Cache(Redis) miss for key {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache(Redis) hit for key {Key}", key);
                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache(Redis) for key {Key}", key);
                return null;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Removed key {Key} from cache(Redis)", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key {Key} from cache(Redis)", key);
            }
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                    options.SetAbsoluteExpiration(expiration.Value);
                else
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // значение по умолчанию

                var data = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, data, options);

                _logger.LogDebug("Set key {Key} in cache(Redis) with expiration {Expiration}",
                    key, expiration?.ToString() ?? "30 minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache(Redis) for key {Key}", key);
            }
        }
    }
}

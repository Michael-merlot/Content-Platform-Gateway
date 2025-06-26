using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gateway.Core.Interfaces.Persistence;

namespace Gateway.Infrastructure.Persistence.MultiLevel
{
    public class MultiLevelCacheRepository : IMultiLevelCacheRepository
    {
        private readonly IMemoryCacheRepository _memoryCache;
        private readonly IDistributedCacheService _distributedCache;

        public MultiLevelCacheRepository(IMemoryCacheRepository memoryCache, IDistributedCacheService distributedCache)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _memoryCache.GetAsync<T>(key);
            if (value != null) return value;

            value = await _distributedCache.GetAsync<T>(key);
            if (value != null)
                await _memoryCache.SetAsync(key, value);

            return value;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            await _memoryCache.SetAsync(key, value, expiration);
            await _distributedCache.SetAsync(key, value, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _memoryCache.RemoveAsync(key);
            await _distributedCache.RemoveAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (await _memoryCache.ExistsAsync(key)) return true;
            return await _distributedCache.ExistsAsync(key);
        }
    }
}

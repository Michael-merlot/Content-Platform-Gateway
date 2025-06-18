using System;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Persistence
{

    public interface IDistributedCacheService
    {

        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}

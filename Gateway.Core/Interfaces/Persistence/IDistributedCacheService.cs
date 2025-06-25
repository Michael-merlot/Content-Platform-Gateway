using System;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Persistence
{

    public interface IDistributedCacheService 
    {

        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}

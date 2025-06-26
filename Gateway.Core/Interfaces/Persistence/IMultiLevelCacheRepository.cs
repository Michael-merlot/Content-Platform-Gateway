using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Persistence
{
    /// <summary>
    /// Интерфейс для многоуровневого (in-memory + distributed) кеша.
    /// </summary>
    public interface IMultiLevelCacheRepository
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}

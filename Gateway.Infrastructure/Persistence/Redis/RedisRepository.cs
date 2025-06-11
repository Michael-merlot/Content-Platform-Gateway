using Gateway.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Gateway.Infrastructure.Persistence.Redis
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisRepository> _logger;
        private readonly IDatabase _db;

        public RedisRepository(IConnectionMultiplexer redis, ILogger<RedisRepository> logger)
        {
            _redis = redis;
            _logger = logger;
            _db = redis.GetDatabase();
        }

        /// <summary>
        /// получает значение по ключу
        /// </summary>
        /// <typeparam name="T">тип значения</typeparam>
        /// <param name="key">ключ</param>
        /// <returns>значение или null, если ключ не найден</returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from Redis cache for key {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// сохраняет значение по ключу
        /// </summary>
        /// <typeparam name="T">тип значения</typeparam>
        /// <param name="key">ключ</param>
        /// <param name="value">значение</param>
        /// <param name="expiry">время жизни (TTL) кэша</param>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, json, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving data to Redis cache for key {Key}", key);
            }
        }

        /// <summary>
        /// удаляет значение по ключу
        /// </summary>
        /// <param name="key">ключ</param>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing data from Redis cache for key {Key}", key);
            }
        }

        /// <summary>
        /// проверяет наличие ключа
        /// </summary>
        /// <param name="key">ключ</param>
        /// <returns>True, если ключ существует</returns>
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking key existence in Redis cache for key {Key}", key);
                return false;
            }
        }
    }
}

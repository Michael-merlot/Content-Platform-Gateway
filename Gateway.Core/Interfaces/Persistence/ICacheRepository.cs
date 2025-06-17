using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Persistence
{
    /// <summary>
    /// Интерфейс универсального кэш-репозитория.
    /// Предоставляет методы для работы с кэшем (получение, установка, удаление, проверка наличия ключа).
    /// Используется для абстракции кэширования, например, через Redis.
    /// </summary>
    public interface ICacheRepository
    {
        /// <summary>
        /// Получить значение из кэша по ключу.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <returns>Десериализованное значение или null, если ключ не найден</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Сохранить значение в кэше по ключу.
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="value">Сохраняемое значение</param>
        /// <param name="expiry">Время жизни (TTL) кэша. Если не указано — значение бессрочное.</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Удалить значение из кэша по ключу.
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Проверить, существует ли ключ в кэше.
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        /// <returns>True, если ключ существует, иначе False</returns>
        Task<bool> ExistsAsync(string key);
    }
}

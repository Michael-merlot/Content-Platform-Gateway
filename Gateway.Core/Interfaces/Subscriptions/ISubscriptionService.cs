using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gateway.Core.Models.Subscriptions;
using System.Threading;

namespace Gateway.Core.Interfaces.Subscriptions
{
    /// <summary>
    /// Интерфейс сервиса для получения персонализированной ленты подписок пользователя.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Получить персонализированную ленту контента пользователя по userID.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Коллекция элементов ленты</returns>
        Task<IEnumerable<SubscriptionItem>> GetUserFeedAsync(int userId);

        /// <summary>
        /// Для временной инвалидации.
        /// </summary>
        Task InvalidateUserFeedCacheAsync(int userId);
    }
}

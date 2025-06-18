using System;

namespace Gateway.Api.Models.Subscriptions
{
    /// <summary>
    /// Модель запроса для получения ленты подписок пользователя.
    /// </summary>
    public class SubscriptionRequest
    {
        /// <summary>
        /// Идентификатор пользователя для персонализации ленты.
        /// </summary>
        public int UserId { get; set; }
    }
}

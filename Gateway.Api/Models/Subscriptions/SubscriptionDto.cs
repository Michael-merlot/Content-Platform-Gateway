using System;
using System.Collections.Generic;

namespace Gateway.Api.Models.Subscriptions
{
    /// <summary>
    /// DTO для передачи информации о подписках пользователя в API-ответе.
    /// Включает каналы и их видео.
    /// </summary>
    public class SubscriptionDto
    {
        /// <summary>
        /// Идентификатор канала
        /// </summary>
        public Guid ChannelId { get; set; }

        /// <summary>
        /// Название канала
        /// </summary>
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// Список видео канала
        /// </summary>
        public List<ChannelContentDto> Videos { get; set; } = new();

        /// <summary>
        /// DTO для отдельного видео в ленте подписок пользователя.
        /// </summary>
        public class ChannelContentDto
        {
            /// <summary>
            /// Идентификатор видео
            /// </summary>
            public Guid VideoId { get; set; }

            /// <summary>
            /// Название видео
            /// </summary>
            public string Title { get; set; } = string.Empty;

            /// <summary>
            /// Описание видео
            /// </summary>
            public string Description { get; set; } = string.Empty;

            /// <summary>
            /// Дата публикации видео
            /// </summary>
            public DateTime PublishedAt { get; set; }

            /// <summary>
            /// Релевантность видео (для сортировки)
            /// </summary>
            public double RelevanceScore { get; set; }
        }
    }
}

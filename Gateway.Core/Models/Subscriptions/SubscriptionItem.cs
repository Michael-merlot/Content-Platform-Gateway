using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Gateway.Core.Models.Subscriptions
{
    /// <summary>
    /// Доменная модель элемента ленты подписок пользователя.
    /// Описывает канал и его контент.
    /// </summary>
    public class SubscriptionItem
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
        /// Список видео (контента) канала
        /// </summary>
        public List<ChannelContentItem> Videos { get; set; } = new();

        /// <summary>
        /// Доменная модель видео-элемента в ленте подписок.
        /// </summary>
        public class ChannelContentItem
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
            /// Релевантность видео (используется для ранжирования)
            /// </summary>
            public double RelevanceScore { get; set; }
        }
    }
}

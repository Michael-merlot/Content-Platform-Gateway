using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Core.Interfaces.Subscriptions;
using Gateway.Core.Interfaces.Persistence;
using Gateway.Core.Models.Subscriptions;

namespace Gateway.Core.Services.Subscriptions
{
    /// <summary>
    /// Реализация сервиса для получения персонализированной ленты подписок пользователя.
    /// Интегрируется с кэшем и реализует ранжирование контента.
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ICacheRepository _cache;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Конструктор сервиса, внедряет кэш-репозиторий.
        /// </summary>
        /// <param name="cache">Кэш-репозиторий</param>
        public SubscriptionService(ICacheRepository cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Получить персонализированную ленту пользователя.
        /// Сначала пробует взять из кэша, если нет — формирует новую, ранжирует и кладёт в кэш.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Коллекция каналов с видео, отсортированных по релевантности</returns>
        public async Task<IEnumerable<SubscriptionItem>> GetUserFeedAsync(Guid userId)
        {
            var cacheKey = $"user_feed_{userId}";

            // 1. Пробуем взять данные из кэша по ключу userId
            var cached = await _cache.GetAsync<List<SubscriptionItem>>(cacheKey);
            if (cached != null)
                return cached;

            // 2. Формируем ленту для пользователя (заглушка — реальные данные могут браться из БД)
            var now = DateTime.UtcNow;
            var feed = new List<SubscriptionItem>
            {
                new SubscriptionItem
                {
                    ChannelId = Guid.NewGuid(),
                    ChannelName = "Мир IT",
                    Videos = new List<SubscriptionItem.ChannelContentItem>
                    {
                        new SubscriptionItem.ChannelContentItem
                        {
                            VideoId = Guid.NewGuid(),
                            Title = "Последние новости AI",
                            Description = "Тренд 'А вы знали, что вы искусственный интеллект?'.",
                            PublishedAt = now.AddHours(-1),
                            RelevanceScore = 0
                        },
                        new SubscriptionItem.ChannelContentItem
                        {
                            VideoId = Guid.NewGuid(),
                            Title = "AI в робототехнике",
                            Description = "Пасхалка.",
                            PublishedAt = now.AddHours(-10),
                            RelevanceScore = 0
                        }
                    }
                },
                new SubscriptionItem
                {
                    ChannelId = Guid.NewGuid(),
                    ChannelName = "Обзоры фильмов",
                    Videos = new List<SubscriptionItem.ChannelContentItem>
                    {
                        new SubscriptionItem.ChannelContentItem
                        {
                            VideoId = Guid.NewGuid(),
                            Title = "Топ 10 фильмов в жанре фэнтези",
                            Description = "Лучшие фантастические фильмы для просмотра",
                            PublishedAt = now.AddHours(-5),
                            RelevanceScore = 0
                        }
                    }
                },
                new SubscriptionItem
                {
                    ChannelId = Guid.NewGuid(),
                    ChannelName = "Мистер Бист",
                    Videos = new List<SubscriptionItem.ChannelContentItem>
                    {
                        new SubscriptionItem.ChannelContentItem
                        {
                            VideoId = Guid.NewGuid(),
                            Title = "Я сбросил 250 тысяч тонн тротила",
                            Description = "Не пасхалка.",
                            PublishedAt = now.AddDays(-1),
                            RelevanceScore = 0
                        }
                    }
                }
            };

            // 3. Ранжируем видео внутри каналов по релевантности (чем свежее — тем выше score)
            foreach (var channel in feed)
            {
                foreach (var video in channel.Videos)
                {
                    var hoursAgo = (now - video.PublishedAt).TotalHours;
                    video.RelevanceScore = Math.Max(0, 100 - hoursAgo * 10);
                }
                channel.Videos = channel.Videos.OrderByDescending(v => v.RelevanceScore).ToList();
            }

            // 4. Сохраняем сформированную ленту в кэш на 10 минут
            await _cache.SetAsync(cacheKey, feed, CacheTtl);

            return feed;
        }
    }
}

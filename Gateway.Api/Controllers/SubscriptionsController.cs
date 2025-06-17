using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Gateway.Core.Interfaces.Subscriptions;
using Gateway.Api.Models.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers
{
    /// <summary>
    /// Контроллер для работы с лентой подписок пользователя.
    /// Предоставляет API для получения персонализированной ленты контента.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        /// <summary>
        /// Конструктор контроллера, внедряет сервис подписок.
        /// </summary>
        /// <param name="subscriptionService">Сервис работы с подписками</param>
        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Получить персонализированную ленту подписок пользователя.
        /// </summary>
        /// <param name="request">Модель запроса с userId</param>
        /// <returns>Список каналов с видео, отсортированных по релевантности</returns>
        [HttpPost("feed")]
        public async Task<ActionResult<List<SubscriptionDto>>> GetFeed([FromBody] SubscriptionRequest request)
        {
            if (request == null || request.UserId == Guid.Empty)
                return BadRequest("UserId is required.");

            // Получаем ленту пользователя через сервис подписок.
            var feed = await _subscriptionService.GetUserFeedAsync(request.UserId);

            // Преобразуем доменные модели к DTO для ответа API.
            var result = feed.Select(channel => new SubscriptionDto
            {
                ChannelId = channel.ChannelId,
                ChannelName = channel.ChannelName,
                Videos = channel.Videos.Select(video => new SubscriptionDto.ChannelContentDto
                {
                    VideoId = video.VideoId,
                    Title = video.Title,
                    Description = video.Description,
                    PublishedAt = video.PublishedAt,
                    RelevanceScore = video.RelevanceScore
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}

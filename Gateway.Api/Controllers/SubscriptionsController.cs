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
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

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

            // ---- HTTP CACHING ----

            // 1. Генерируем ETag по сериализованному результату
            string resultJson = JsonSerializer.Serialize(result);
            string etag;
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(resultJson));
                etag = $"\"{Convert.ToBase64String(hash)}\""; 
            }

            // 2. Определяем Last-Modified
            DateTime? lastModified = null;
            if (result.Count > 0)
            {
                lastModified = result
                    .SelectMany(r => r.Videos)
                    .Select(v => v.PublishedAt)
                    .DefaultIfEmpty(DateTime.MinValue)
                    .Max();
            }

            // 3. Проверяем заголовки If-None-Match и If-Modified-Since
            if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == etag)
                return StatusCode(StatusCodes.Status304NotModified);

            if (lastModified.HasValue &&
                Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceStr) &&
                DateTime.TryParse(ifModifiedSinceStr, out var ifModifiedSince) &&
                lastModified.Value <= ifModifiedSince.ToUniversalTime())
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // 4. Устанавливаем ETag и Last-Modified
            Response.Headers["ETag"] = etag;
            if (lastModified.HasValue)
                Response.Headers["Last-Modified"] = lastModified.Value.ToUniversalTime().ToString("R");

            return Ok(result);
        }
        /// <summary>
        /// Для временной инициации инвалидации.
        /// </summary>
        [HttpPost("invalidate")]
        public async Task<IActionResult> InvalidateUserFeed([FromBody] Guid userId)
        {
            await _subscriptionService.InvalidateUserFeedCacheAsync(userId);
            return Ok($"Инвалидация кэша для userId {userId} инициирована.");
        }
    }
}

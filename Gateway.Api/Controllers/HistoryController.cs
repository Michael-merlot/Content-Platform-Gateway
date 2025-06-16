using Gateway.Api.Models.History;
using Gateway.Core.Interfaces.History;
using Gateway.Core.Models.History;
using Microsoft.AspNetCore.Mvc;
namespace Gateway.Api.Controllers
{
    [ApiController]
    //класс для общения по сети
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpPost("test")]
        [ProducesResponseType(typeof(AddHistoryResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AddHistoryResponse>> AddHistory(AddHistoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var historyItem = new HistoryItem
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ContentId = request.ContentId,
                ContentType = request.ContentType,
                ViewedAt = DateTimeOffset.UtcNow
            };
            var addedItem = await _historyService.AddHistoryItemAsync(historyItem);

            var addedItemDto = new AddHistoryResponse
            {
                Id = addedItem.Id,
                UserId = addedItem.UserId,
                ContentId = addedItem.ContentId,
                ContentType = addedItem.ContentType,
                ViewedAt = addedItem.ViewedAt
            };

            return CreatedAtAction(nameof(GetHistoryByUserId), new { userId = addedItemDto.UserId }, addedItemDto);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<AddHistoryResponse>>> GetHistoryByUserId(
            Guid userId,
            [FromQuery] ContentType? contentType = null)
        {
            var historyItems = await _historyService.GetUserHistoryAsync(userId, contentType);

            if (!historyItems.Any())
            {
                return NotFound($"No history found for user ID {userId}" +
                                (contentType.HasValue ? $" with content type {contentType.Value}" : ""));
            }

            var historyItemDtos = historyItems.Select(item => new AddHistoryResponse
            {
                Id = item.Id,
                UserId = item.UserId,
                ContentId = item.ContentId,
                ContentType = item.ContentType,
                ViewedAt = item.ViewedAt
            }).ToList();
            return Ok(historyItemDtos);
        }
        /// <summary>
        /// Получает количество просмотров для конкретного Content ID.
        /// </summary>
        /// <param name="contentId">Уникальный идентификатор контента.</param>
        /// <returns>Количество просмотров.</returns>
        /// <response code="200">Возвращает количество просмотров.</response>
        /// <response code="400">Если Content ID невалиден.</response>
        [HttpGet("views/content/{contentId}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<int>> GetViewsByContentId([FromRoute] Guid contentId)
        {
            if (contentId == Guid.Empty)
            {
                return BadRequest("Content ID cannot be empty.");
            }
            var viewsCount = await _historyService.GetViewsCountByContentIdAsync(contentId);
            return Ok(viewsCount);
        }

        /// <summary>
        /// Получает количество просмотров для конкретного типа контента.
        /// </summary>
        /// <param name="contentType">Тип контента (например, Video=1, Article=2).</param>
        /// <returns>Количество просмотров.</returns>
        /// <response code="200">Возвращает количество просмотров.</response>
        /// <response code="400">Если тип контента невалиден.</response>
        [HttpGet("views/type/{contentType}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<int>> GetViewsByContentType([FromRoute] ContentType contentType)
        {
            if (contentType == ContentType.Unknown)
            {
                return BadRequest("Content Type cannot be Unknown.");
            }
            var viewsCount = await _historyService.GetViewsCountByContentTypeAsync(contentType);
            return Ok(viewsCount);
        }
    }
}

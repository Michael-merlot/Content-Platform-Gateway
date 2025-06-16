using Gateway.Api.Models.History;
using Gateway.Core.Interfaces.History;
using Gateway.Core.Models.History;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers
{
    [ApiController]
    [Route("api/v1/history")]  
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        /// <summary>
        /// Добавляет новую запись в историю просмотров
        /// </summary>
        [HttpPost]  
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

        /// <summary>
        /// Получает историю просмотров пользователя по ID
        /// </summary>
        [HttpGet("user/{userId}")]  
        [ProducesResponseType(typeof(IEnumerable<AddHistoryResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<AddHistoryResponse>>> GetHistoryByUserId(
            Guid userId,
            [FromQuery] ContentType? contentType = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)  
        {
            var historyItems = await _historyService.GetUserHistoryAsync(userId, contentType, skip, take);

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
    }
}

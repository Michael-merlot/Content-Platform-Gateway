using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gateway.Core.Interfaces.Notifications;
//using Gateway.Core.DTOs.Notifications; // Используем DTO из Core
using Gateway.Api.Models.Notifications; // Используем модели запросов из Api

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // Добавьте, когда будет настроена авторизация
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Получить уведомления для текущего пользователя.
    /// </summary>
    /// <param name="isRead">Фильтр по статусу прочитано/не прочитано (опционально).</param>
    /// <returns>Список уведомлений.</returns>
    [HttpGet]
    // Примечание: UserId здесь должен быть получен из JWT токена после авторизации,
    // а не передаваться в запросе. Для простоты сейчас передаем.
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications([FromQuery] Guid userId, [FromQuery] bool? isRead = null)
    {
        // Пример: Guid currentUserId = Guid.Parse(User.Identity.Name); // Получить ID из JWT
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, isRead);
        return Ok(notifications);
    }

    /// <summary>
    /// Отметить уведомление как прочитанное.
    /// </summary>
    /// <param name="notificationId">Идентификатор уведомления.</param>
    /// <returns>No Content.</returns>
    [HttpPut("{notificationId}/read")]
    public async Task<ActionResult> MarkAsRead(Guid notificationId, [FromQuery] Guid userId) // userId также должен браться из авторизации
    {
        try
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex) // Можете использовать NotificationException
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Отметить все уведомления пользователя как прочитанные.
    /// </summary>
    /// <returns>No Content.</returns>
    [HttpPut("mark-all-read")]
    public async Task<ActionResult> MarkAllRead([FromQuery] Guid userId) // userId также должен браться из авторизации
    {
        try
        {
            await _notificationService.MarkAllUserNotificationsAsReadAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Удалить уведомление.
    /// </summary>
    /// <param name="notificationId">Идентификатор уведомления.</param>
    /// <returns>No Content.</returns>
    [HttpDelete("{notificationId}")]
    public async Task<ActionResult> DeleteNotification(Guid notificationId, [FromQuery] Guid userId) // userId также должен браться из авторизации
    {
        try
        {
            await _notificationService.DeleteNotificationAsync(notificationId, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Создать новое уведомление (только для тестирования).
    /// </summary>
    /// <param name="request">Данные для создания уведомления.</param>
    /// <returns>Созданное уведомление.</returns>
    [HttpPost]
    // [Authorize(Roles = "Admin")] // Только для администраторов
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        var notification = await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            request.Message,
            request.Type,
            request.RelatedEntityId
        );
        // Возвращаем 201 Created с заголовком Location
        return CreatedAtAction(nameof(GetUserNotifications), new { userId = notification.UserId, id = notification.Id }, notification);
    }
}

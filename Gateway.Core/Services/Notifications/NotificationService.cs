//using Gateway.Core.DTOs.Notifications;
using Gateway.Core.Interfaces.Notifications;
using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Models.Notifications;
using Gateway.Core.DTOs;

namespace Gateway.Core.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationRealtimeClient _notificationRealtimeClient;

    public NotificationService(INotificationRepository notificationRepository, INotificationRealtimeClient notificationRealtimeClient)
    {
        _notificationRepository = notificationRepository;
        _notificationRealtimeClient = notificationRealtimeClient;
    }

    public async Task<NotificationDto> CreateAndSendNotificationAsync(Guid userId, string message, NotificationType type, Guid? relatedEntityId = null)
    {
        var notification = new Notification(userId, message, type, relatedEntityId);
        await _notificationRepository.AddAsync(notification);

        var notificationDto = MapToDto(notification);

        // Отправляем мгновенное уведомление через WebSocket/SignalR
        await _notificationRealtimeClient.SendNotificationToUserAsync(userId, notificationDto);

        return notificationDto;
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
        {
            throw new InvalidOperationException("Notification not found or not authorized.");
        }

        if (!notification.IsRead)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task MarkAllUserNotificationsAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _notificationRepository.GetByUserIdAndStatusAsync(userId, false);
        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null)
    {
        IEnumerable<Notification> notifications;
        if (isRead.HasValue)
        {
            notifications = await _notificationRepository.GetByUserIdAndStatusAsync(userId, isRead.Value);
        }
        else
        {
            notifications = await _notificationRepository.GetByUserIdAsync(userId);
        }

        return notifications.Select(n => MapToDto(n)).ToList();
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
        {
            throw new InvalidOperationException("Notification not found or not authorized.");
        }
        await _notificationRepository.DeleteAsync(notification);
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Message = notification.Message,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt,
            IsRead = notification.IsRead,
            RelatedEntityId = notification.RelatedEntityId
        };
    }
}

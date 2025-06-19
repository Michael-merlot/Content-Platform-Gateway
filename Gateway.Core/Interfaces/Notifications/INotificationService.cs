using Gateway.Core.Models.Notifications;
using Gateway.Core.DTOs;

namespace Gateway.Core.Interfaces.Notifications;

public interface INotificationService
{

    Task<NotificationDto> CreateAndSendNotificationAsync(Guid userId, string message, NotificationType type, Guid? relatedEntityId = null);
    Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllUserNotificationsAsReadAsync(Guid userId);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead = null);
    Task DeleteNotificationAsync(Guid notificationId, Guid userId);

}

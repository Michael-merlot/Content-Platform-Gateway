using Gateway.Core.Models.Notifications;

namespace Gateway.Core.Interfaces.Notifications;
public interface INotificationRepository
{
    Task AddAsync(Notification notification);//Post
    Task UpdateAsync(Notification notification);//PUT
    Task DeleteAsync(Notification notification);//Delete
    Task<Notification> GetByIdAsync(Guid notificationId);//GET
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);//GET
    Task<IEnumerable<Notification>> GetByUserIdAndStatusAsync(Guid userId, bool isRead);//GET
}

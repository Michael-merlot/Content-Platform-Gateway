using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Core.Interfaces.Notifications;
using Gateway.Core.Models.Notifications;

namespace Gateway.Infrastructure.Persistence.InMemory
{
    public class InMemoryNotificationRepository : INotificationRepository
    {
        private static readonly ConcurrentDictionary<Guid, Notification> _notifications = new ConcurrentDictionary<Guid, Notification>();

        public Task AddAsync(Notification notification)
        {
            _notifications.TryAdd(notification.Id, notification);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Notification notification)
        {
            _notifications.AddOrUpdate(notification.Id, notification, (key, existingVal) => notification);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Notification notification)
        {
            _notifications.TryRemove(notification.Id, out _);
            return Task.CompletedTask;
        }

        public Task<Notification> GetByIdAsync(Guid notificationId)
        {
            _notifications.TryGetValue(notificationId, out var notification);
            return Task.FromResult(notification);
        }

        public Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            var userNotifications = _notifications.Values.Where(n => n.UserId == userId).ToList();
            return Task.FromResult<IEnumerable<Notification>>(userNotifications);
        }

        public Task<IEnumerable<Notification>> GetByUserIdAndStatusAsync(Guid userId, bool isRead)
        {
            var userNotifications = _notifications.Values.Where(n => n.UserId == userId && n.IsRead == isRead).ToList();
            return Task.FromResult<IEnumerable<Notification>>(userNotifications);
        }
    }
}

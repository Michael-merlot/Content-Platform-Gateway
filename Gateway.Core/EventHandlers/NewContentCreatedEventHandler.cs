using Gateway.Core.Events;
using Gateway.Core.Interfaces.Notifications;
using Gateway.Core.Models.Notifications;

namespace Gateway.Core.EventHandlers;

public class NewContentCreatedEventHandler
{
    private readonly INotificationService _notificationService;

    public NewContentCreatedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(NewContentCreatedEvent @event)
    {
        // Отправляем уведомления всем подписчикам
        if (@event.SubscriberIds != null)
        {
            foreach (var subscriberId in @event.SubscriberIds)
            {
                var message = $"Новый контент '{@event.ContentTitle}' от автора {@event.AuthorId} был опубликован!";
                await _notificationService.CreateAndSendNotificationAsync(
                    subscriberId,
                    message,
                    NotificationType.NewContent,
                    @event.ContentId
                );
            }
        }
        // можно отправить уведомление, потом продумать
        // await _notificationService.CreateAndSendNotificationAsync(Guid.NewGuid(), $"Новый контент создан: {@event.ContentTitle}", NotificationType.SystemEvent);
    }
}

using Gateway.Core.Models.Notifications;

namespace Gateway.Core.DTOs;

//должен быть в в Api/Models/Notifications, но остальные файлы не видят.
public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public Guid? RelatedEntityId { get; set; }
}

using Gateway.Core.Models.Notifications;

namespace Gateway.Core.DTOs;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.Models.Notifications;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Message { get; private set; }
    public NotificationType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRead { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private Notification() { } // Приватный конструктор для EF Core

    public Notification(Guid userId, string message, NotificationType type, Guid? relatedEntityId = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Message = message;
        Type = type;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
        RelatedEntityId = relatedEntityId;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
    }
}

public enum NotificationType
{
    NewContent,
    Comment,
    Like,
    SystemEvent,
}


using System;

namespace Gateway.Core.Models.Notifications;

public abstract class NotificationItem
{
    public Guid Id { get; protected set; }
    public Guid UserId { get; protected set; }
    public string Type { get; protected set; } // Тип уведомления
    public string Message { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; protected set; } = false;

    // Приватный конструктор для EF Core
    protected NotificationItem()
    {
        // Устанавливаем CreatedAt и IsRead значения по умолчанию здесь
        CreatedAt = DateTimeOffset.UtcNow;
        IsRead = false;
    }

    // Защищенный конструктор для инициализации из дочерних классов
    protected NotificationItem(Guid userId, string type, string message)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        CreatedAt = DateTimeOffset.UtcNow;
        IsRead = false;
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

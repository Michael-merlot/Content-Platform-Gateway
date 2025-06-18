using System;

namespace Gateway.Core.Models.Notifications;

public abstract class NotificationItem
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; } // Тип уведомления
    public string Message { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? TargetUrl { get; set; } // URL, куда ведет уведомление (необязательно)

    // Конструктор для EF Core и инициализации
    protected NotificationItem(string userId, string type, string message, string? targetUrl = null)
    {
        Id = Guid.NewGuid();
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        TargetUrl = targetUrl;
    }

    // Для EF Core, чтобы он мог создавать экземпляры
    protected NotificationItem() { }
}

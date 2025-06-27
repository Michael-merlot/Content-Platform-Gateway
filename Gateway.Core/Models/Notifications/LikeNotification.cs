namespace Gateway.Core.Models.Notifications;

public class LikeNotification : NotificationItem
{
    public string LikedByUsername { get; set; }
    public Guid ContentId { get; set; }

    public LikeNotification(Guid userId, string message, string likedByUsername, Guid contentId)
            : base(userId, "Like", message)
    {
        LikedByUsername = likedByUsername ?? throw new ArgumentNullException(nameof(likedByUsername));
        ContentId = contentId;
    }

    // Приватный конструктор для EF Core
    public LikeNotification() : base() { }
}

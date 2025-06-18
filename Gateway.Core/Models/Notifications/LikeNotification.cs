namespace Gateway.Core.Models.Notifications;

public class LikeNotification : NotificationItem
{
    public string LikedByUsername { get; set; }
    public Guid ContentId { get; set; }

    public LikeNotification(string userId, string message, string likedByUsername, Guid contentId, string? targetUrl = null)
        : base(userId, "Like", message, targetUrl)
    {
        LikedByUsername = likedByUsername ?? throw new ArgumentNullException(nameof(likedByUsername));
        ContentId = contentId;
    }

    // Для EF Core
    public LikeNotification() : base() { }
}

namespace Gateway.Core.Models.Notifications;

// Пример уведомления о новом комментарии
public class CommentNotification : NotificationItem
{
    public string CommentText { get; set; }
    public string CommentAuthor { get; set; }
    public Guid ContentId { get; set; } // ID контента, к которому относится комментарий

    public CommentNotification(string userId, string message, string commentText, string commentAuthor, Guid contentId, string? targetUrl = null)
        : base(userId, "Comment", message, targetUrl)
    {
        CommentText = commentText ?? throw new ArgumentNullException(nameof(commentText));
        CommentAuthor = commentAuthor ?? throw new ArgumentNullException(nameof(commentAuthor));
        ContentId = contentId;
    }

    // Для EF Core
    public CommentNotification() : base() { }
}

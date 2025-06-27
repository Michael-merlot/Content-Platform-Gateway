namespace Gateway.Core.Models.Notifications;

// Пример уведомления о новом комментарии
public class CommentNotification : NotificationItem
{
    public string CommentText { get; protected set; }
    public string CommentAuthor { get; protected set; }
    public Guid ContentId { get; protected set; } // ID контента, к которому относится комментарий
    public CommentNotification(Guid userId, string message, string commentText, string commentAuthor, Guid contentId)
        : base(userId, "Comment", message)
    {
        CommentText = commentText ?? throw new ArgumentNullException(nameof(commentText));
        CommentAuthor = commentAuthor ?? throw new ArgumentNullException(nameof(commentAuthor));
        ContentId = contentId;
    }

    // Приватный конструктор для EF Core
    private CommentNotification() : base() { }
}

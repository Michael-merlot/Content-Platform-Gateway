using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.Models.Notifications;
public class NewContentNotification : NotificationItem
{
    public string ContentTitle { get; protected set; }
    public string ContentAuthor { get; protected set; }
    public Guid ContentId { get; protected set; }

    public NewContentNotification(Guid userId, string message, string contentTitle, string contentAuthor, Guid contentId)
            : base(userId, "NewContent", message)
    {
        ContentTitle = contentTitle ?? throw new ArgumentNullException(nameof(contentTitle));
        ContentAuthor = contentAuthor ?? throw new ArgumentNullException(nameof(contentAuthor));
        ContentId = contentId;
    }
    // Для EF Core
    public NewContentNotification() : base() { }
}

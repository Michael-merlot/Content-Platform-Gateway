using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.Models.Notifications;
public class NewContentNotification : NotificationItem
{
    public string ContentTitle { get; set; }
    public string ContentAuthor { get; set; }

    public NewContentNotification(string userId, string message, string contentTitle, string contentAuthor, string? targetUrl = null)
        : base(userId, "NewContent", message, targetUrl)
    {
        ContentTitle = contentTitle ?? throw new ArgumentNullException(nameof(contentTitle));
        ContentAuthor = contentAuthor ?? throw new ArgumentNullException(nameof(contentAuthor));
    }

    // Для EF Core
    public NewContentNotification() : base() { }
}

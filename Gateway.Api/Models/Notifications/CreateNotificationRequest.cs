using Gateway.Core.Models.Notifications;
using System;
using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Notifications;

public class CreateNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public Guid? RelatedEntityId { get; set; }
}

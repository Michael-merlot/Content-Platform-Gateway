using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gateway.Core.DTOs;
namespace Gateway.Core.Interfaces.Clients;

public interface INotificationRealtimeClient
{
    Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);
    Task SendNotificationToAllAsync(NotificationDto notification);
}

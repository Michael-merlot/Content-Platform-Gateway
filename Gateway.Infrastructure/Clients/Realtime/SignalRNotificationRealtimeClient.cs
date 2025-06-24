using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Gateway.Core.DTOs;
using Gateway.Core.Interfaces.Clients;

namespace Gateway.Infrastructure.Clients.Realtime
{
    public class SignalRNotificationRealtimeClient : INotificationRealtimeClient
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationRealtimeClient(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
        {
            // если  SignalR Hub авторизован и User.Identity.Name это Guid пользователя:
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Notification sent to user {userId} via SignalR: {notification.Message}");
        }

        public async Task SendNotificationToAllAsync(NotificationDto notification)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Notification sent to all via SignalR: {notification.Message}");
        }
    }
}

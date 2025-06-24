using Microsoft.AspNetCore.SignalR;

namespace Gateway.Infrastructure.Clients.Realtime;

public class NotificationHub : Hub
{
    // В реальном приложении здесь будет логика авторизации для Context.User.Identity.Name
    // Context.User.Identity.Name будет соответствовать ID пользователя (строка)
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"SignalR Client connected: {Context.ConnectionId}");
        // Можно добавить пользователя в группу, если у вас есть информация о его ID в момент подключения
        // string userId = Context.User?.Identity?.Name; // Получаем ID пользователя из клеймов (если авторизован)
        // if (!string.IsNullOrEmpty(userId))
        // {
        //     Groups.AddToGroupAsync(Context.ConnectionId, userId);
        // }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"SignalR Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendTestMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}

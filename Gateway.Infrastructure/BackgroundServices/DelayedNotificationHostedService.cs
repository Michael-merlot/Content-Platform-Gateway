using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Core.Interfaces.Notifications;
using Gateway.Core.Models.Notifications;

namespace Gateway.Infrastructure.BackgroundServices
{
    public class DelayedNotificationHostedService : BackgroundService
    {
        private readonly ILogger<DelayedNotificationHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public DelayedNotificationHostedService(ILogger<DelayedNotificationHostedService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Delayed Notification Hosted Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Delayed Notification Service working at: {time}", DateTimeOffset.Now);

                // пример - отправка тестового уведомления каждые 30 секунд условному пользователю.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    try
                    {
                        // Здесь просто пример
                        var testUserId = Guid.Parse("A1B2C3D4-E5F6-7890-1234-567890ABCDEF");
                        await notificationService.CreateAndSendNotificationAsync(
                            testUserId,
                            $"Автоматическое уведомление: Новая информация доступна! ({DateTime.Now})",
                            NotificationType.SystemEvent
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending delayed notification from hosted service.");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("Delayed Notification Hosted Service stopped.");
        }
    }
}

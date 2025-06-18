using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gateway.Core.Models.Cache;
using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Gateway.Infrastructure.Services.Cache
{
    /// <summary>
    /// Слушатель Pub/Sub для инвалидации in-memory кэша на всех инстансах.
    /// </summary>
    public class RedisCacheInvalidationListener : BackgroundService
    {
        private readonly IMemoryCacheRepository _memoryCache;
        private readonly ISubscriber _subscriber;
        private readonly ILogger<RedisCacheInvalidationListener> _logger;

        public RedisCacheInvalidationListener(
            IConnectionMultiplexer redis,
            IMemoryCacheRepository memoryCache,
            ILogger<RedisCacheInvalidationListener> logger)
        {
            _subscriber = redis.GetSubscriber();
            _memoryCache = memoryCache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _subscriber.SubscribeAsync(RedisCacheInvalidator.ChannelName, async (channel, value) =>
            {
                var msg = CacheInvalidationMessage.FromJson(value!);
                if (msg?.Key != null)
                {
                    await _memoryCache.RemoveAsync(msg.Key);
                    _logger.LogInformation("Invalidated memory cache for key '{Key}' via Redis pub/sub", msg.Key);
                }
            });
        }
    }
}

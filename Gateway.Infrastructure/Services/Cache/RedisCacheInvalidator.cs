using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gateway.Core.Interfaces.Cache;
using Gateway.Core.Models.Cache;
using StackExchange.Redis;

namespace Gateway.Infrastructure.Services.Cache
{
    /// <summary>
    /// Сервис для публикации инвалидации кэша через Redis Pub/Sub.
    /// </summary>
    public class RedisCacheInvalidator : ICacheInvalidator
    {
        private readonly ISubscriber _subscriber;
        public const string ChannelName = "cache-invalidation";

        public RedisCacheInvalidator(IConnectionMultiplexer redis)
        {
            _subscriber = redis.GetSubscriber();
        }

        public async Task PublishInvalidationAsync(string key)
        {
            var message = new CacheInvalidationMessage { Key = key }.ToJson();
            await _subscriber.PublishAsync(ChannelName, message);
        }
    }
}

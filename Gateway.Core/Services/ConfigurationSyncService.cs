using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Core.Services
{

    public class ConfigurationSyncService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationSyncService> _logger;
        private const string CONFIGURATION_CACHE_KEY = "gateway:configuration";
        private const int SYNC_INTERVAL_SECONDS = 60;

        public ConfigurationSyncService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<ConfigurationSyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cacheService = scope.ServiceProvider.GetRequiredService<IDistributedCacheService>();

                    var cachedConfig = await cacheService.GetAsync<Dictionary<string, string>>(CONFIGURATION_CACHE_KEY);
                    var currentConfig = GetCurrentConfiguration();

                    if (NeedsUpdate(cachedConfig, currentConfig))
                    {
                        await cacheService.SetAsync(CONFIGURATION_CACHE_KEY, currentConfig, TimeSpan.FromDays(1));
                        _logger.LogInformation("Configuration synchronized with distributed cache");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error synchronizing configuration");
                }

                await Task.Delay(TimeSpan.FromSeconds(SYNC_INTERVAL_SECONDS), stoppingToken);
            }
        }

        private Dictionary<string, string> GetCurrentConfiguration()
        {
            var config = new Dictionary<string, string>();

            foreach (var setting in _configuration.AsEnumerable())
            {
                if (!string.IsNullOrEmpty(setting.Key) && !string.IsNullOrEmpty(setting.Value))
                {
                    config[setting.Key] = setting.Value;
                }
            }

            return config;
        }

        private bool NeedsUpdate(Dictionary<string, string> cachedConfig, Dictionary<string, string> currentConfig)
        {
            if (cachedConfig == null)
                return true;

            if (cachedConfig.Count != currentConfig.Count)
                return true;

            foreach (var setting in currentConfig)
            {
                if (!cachedConfig.TryGetValue(setting.Key, out var cachedValue) ||
                    cachedValue != setting.Value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

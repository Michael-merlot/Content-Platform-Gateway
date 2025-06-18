using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Configuration;
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
        private readonly IDistributedCacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationSyncService> _logger;
        private const string CONFIGURATION_CACHE_KEY = "gateway:configuration";
        private const int SYNC_INTERVAL_SECONDS = 60;

        public ConfigurationSyncService(
            IDistributedCacheService cacheService,
            IConfiguration configuration,
            ILogger<ConfigurationSyncService> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cachedConfig = await _cacheService.GetAsync<Dictionary<string, string>>(CONFIGURATION_CACHE_KEY);
                    var currentConfig = GetCurrentConfiguration();

                    if (NeedsUpdate(cachedConfig, currentConfig))
                    {
                        await _cacheService.SetAsync(CONFIGURATION_CACHE_KEY, currentConfig, TimeSpan.FromDays(1));
                        _logger.LogInformation("Configuration synchronized with Redis cache");
                    }
                    await ApplyConfigurationChangesFromCache(stoppingToken);
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

        private Task ApplyConfigurationChangesFromCache(CancellationToken stoppingToken)
        {

            return Task.CompletedTask;
        }
    }
}

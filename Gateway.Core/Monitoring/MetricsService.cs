using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Core.Monitoring
{
    public class MetricsService : IHostedService, IDisposable
    {
        private readonly ILogger<MetricsService> _logger;
        private MetricServer _metricServer;

        public static readonly Counter TotalRequests = Metrics
            .CreateCounter("api_gateway_requests_total", "Total number of HTTP requests received");

        public static readonly Counter FailedRequests = Metrics
            .CreateCounter("api_gateway_requests_failed_total", "Total number of failed HTTP requests");

        public static readonly Histogram RequestDuration = Metrics
            .CreateHistogram("api_gateway_request_duration_seconds",
                "Duration of HTTP requests in seconds",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
                });

        public static readonly Gauge ActiveConnections = Metrics
            .CreateGauge("api_gateway_active_connections", "Current number of active connections");

        private readonly Dictionary<string, Gauge> _serviceHealthGauges = new Dictionary<string, Gauge>();

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting metrics server");

            _metricServer = new MetricServer(port: 9091);
            _metricServer.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping metrics server");

            _metricServer?.Stop();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _metricServer?.Dispose();
        }

        public void RegisterServiceHealth(string serviceName)
        {
            if (!_serviceHealthGauges.ContainsKey(serviceName))
            {
                var gauge = Metrics.CreateGauge(
                    $"api_gateway_service_health_{serviceName.ToLowerInvariant()}",
                    $"Health status of {serviceName} service (1=healthy, 0=unhealthy)");

                _serviceHealthGauges[serviceName] = gauge;
            }
        }

        public void SetServiceHealth(string serviceName, bool isHealthy)
        {
            if (_serviceHealthGauges.TryGetValue(serviceName, out var gauge))
            {
                gauge.Set(isHealthy ? 1 : 0);
            }
        }
    }
}

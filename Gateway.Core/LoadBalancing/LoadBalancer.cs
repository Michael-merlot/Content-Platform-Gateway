using Gateway.Core.Interfaces.ServiceDiscovery;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.LoadBalancing
{
    public class LoadBalancer
    {
        private readonly IServiceDiscoveryProvider _serviceDiscoveryProvider;
        private readonly ILogger<LoadBalancer> _logger;
        private readonly LoadBalancingStrategy _strategy;

        private readonly ConcurrentDictionary<string, List<ServiceEndpoint>> _serviceEndpoints = new();
        private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _connectionCounters = new();

        public LoadBalancer(
            IServiceDiscoveryProvider serviceDiscoveryProvider,
            LoadBalancingStrategy strategy,
            ILogger<LoadBalancer> logger)
        {
            _serviceDiscoveryProvider = serviceDiscoveryProvider ?? throw new ArgumentNullException(nameof(serviceDiscoveryProvider));
            _strategy = strategy;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceEndpoint> GetEndpointAsync(string serviceName, string clientIp = null)
        {
            await RefreshEndpointsIfNeededAsync(serviceName);

            if (!_serviceEndpoints.TryGetValue(serviceName, out var endpoints) || !endpoints.Any())
            {
                throw new InvalidOperationException($"No endpoints available for service {serviceName}");
            }

            return _strategy switch
            {
                LoadBalancingStrategy.RoundRobin => GetNextByRoundRobin(serviceName, endpoints),
                LoadBalancingStrategy.LeastConnections => GetByLeastConnections(serviceName, endpoints),
                LoadBalancingStrategy.IpHash => GetByIpHash(serviceName, endpoints, clientIp),
                _ => throw new NotImplementedException($"Strategy {_strategy} not implemented")
            };
        }

        private ServiceEndpoint GetNextByRoundRobin(string serviceName, List<ServiceEndpoint> endpoints)
        {
            int currentIndex = _roundRobinCounters.AddOrUpdate(
                serviceName,
                0,
                (_, count) => (count + 1) % endpoints.Count
            );

            return endpoints[currentIndex];
        }

        private ServiceEndpoint GetByLeastConnections(string serviceName, List<ServiceEndpoint> endpoints)
        {
            var connectionsByEndpoint = _connectionCounters.GetOrAdd(serviceName, _ => new ConcurrentDictionary<string, int>());

            var endpointWithLeastConnections = endpoints
                .OrderBy(e => connectionsByEndpoint.GetOrAdd(e.Url, 0))
                .First();

            connectionsByEndpoint.AddOrUpdate(
                endpointWithLeastConnections.Url,
                1,
                (_, count) => count + 1
            );

            return endpointWithLeastConnections;
        }

        public void DecrementConnectionCount(string serviceName, string endpointUrl)
        {
            if (_connectionCounters.TryGetValue(serviceName, out var connections))
            {
                connections.AddOrUpdate(endpointUrl, 0, (_, count) => Math.Max(0, count - 1));
            }
        }

        private ServiceEndpoint GetByIpHash(string serviceName, List<ServiceEndpoint> endpoints, string clientIp)
        {
            if (string.IsNullOrEmpty(clientIp))
            {
                _logger.LogWarning("Client IP is null or empty, using round robin as fallback");
                return GetNextByRoundRobin(serviceName, endpoints);
            }

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(clientIp));
            var hash = BitConverter.ToInt32(hashBytes, 0);
            var index = Math.Abs(hash) % endpoints.Count;

            return endpoints[index];
        }

        private async Task RefreshEndpointsIfNeededAsync(string serviceName)
        {
            if (!_serviceEndpoints.ContainsKey(serviceName))
            {
                var endpoints = await _serviceDiscoveryProvider.GetServiceEndpointsAsync(serviceName);
                var endpointsList = endpoints.ToList();

                if (!endpointsList.Any())
                {
                    _logger.LogWarning("No endpoints found for service {ServiceName}", serviceName);
                    return;
                }

                _serviceEndpoints[serviceName] = endpointsList;
            }
        }

        public async Task RefreshEndpointsAsync(string serviceName)
        {
            try
            {
                var endpoints = await _serviceDiscoveryProvider.GetServiceEndpointsAsync(serviceName);
                var endpointsList = endpoints.ToList();

                if (!endpointsList.Any())
                {
                    _logger.LogWarning("No endpoints found for service {ServiceName}", serviceName);
                    return;
                }

                _serviceEndpoints[serviceName] = endpointsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing endpoints for service {ServiceName}", serviceName);
            }
        }
    }
}

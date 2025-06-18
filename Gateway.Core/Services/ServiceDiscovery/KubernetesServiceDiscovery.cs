using Gateway.Core.Interfaces.ServiceDiscovery;
using k8s;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Core.Services.ServiceDiscovery
{
    public class KubernetesServiceDiscovery : IServiceDiscoveryProvider
    {
        private readonly Kubernetes _client;
        private readonly ILogger<KubernetesServiceDiscovery> _logger;
        private readonly string _namespace;

        public KubernetesServiceDiscovery(ILogger<KubernetesServiceDiscovery> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                _client = new Kubernetes(config);

                _namespace = Environment.GetEnvironmentVariable("POD_NAMESPACE") ?? "default";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Kubernetes client");
                throw;
            }
        }

        public async Task<IEnumerable<ServiceEndpoint>> GetServiceEndpointsAsync(string serviceName)
        {
            try
            {
                var endpoints = await _client.ReadNamespacedEndpointsAsync(serviceName, _namespace);

                var result = new List<ServiceEndpoint>();

                foreach (var subset in endpoints.Subsets)
                {
                    foreach (var address in subset.Addresses)
                    {
                        foreach (var port in subset.Ports)
                        {
                            result.Add(new ServiceEndpoint
                            {
                                Host = address.Ip,
                                Port = port.Port,
                                ServiceName = serviceName
                            });
                        }
                    }
                }

                _logger.LogInformation("Found {Count} endpoints for service {ServiceName}", result.Count, serviceName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering service endpoints for {ServiceName}", serviceName);
                return Enumerable.Empty<ServiceEndpoint>();
            }
        }
    }
}

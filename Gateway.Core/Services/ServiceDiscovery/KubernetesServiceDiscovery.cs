using Gateway.Core.Interfaces.ServiceDiscovery;
using k8s;
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
            _logger = logger;

            var config = KubernetesClientConfiguration.InClusterConfig();
            _client = new Kubernetes(config);

            _namespace = Environment.GetEnvironmentVariable("POD_NAMESPACE") ?? "default";
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

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering service endpoints for {ServiceName}", serviceName);
                return Enumerable.Empty<ServiceEndpoint>();
            }
        }
    }

    public class ServiceEndpoint
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string ServiceName { get; set; }

        public string Url => $"http://{Host}:{Port}";
    }
}

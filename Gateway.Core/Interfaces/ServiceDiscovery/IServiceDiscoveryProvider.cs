using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.ServiceDiscovery
{
    public interface IServiceDiscoveryProvider
    {
        Task<IEnumerable<ServiceEndpoint>> GetServiceEndpointsAsync(string serviceName);
    }

    public class ServiceEndpoint
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string ServiceName { get; set; }
        public string Url => $"http://{Host}:{Port}";
    }
}

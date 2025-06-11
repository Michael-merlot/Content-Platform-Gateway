using Gateway.Core.Interfaces.Clients;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Clients
{
    public class AiServicesClient : IAiServicesClient
    {
        private readonly HttpClient _httpClient;

        public AiServicesClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

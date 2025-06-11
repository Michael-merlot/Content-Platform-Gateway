using Gateway.Core.Interfaces.Clients;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Clients
{
    public class LaravelApiClient : ILaravelApiClient
    {
        private readonly HttpClient _httpClient;

        public LaravelApiClient(HttpClient httpClient)
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

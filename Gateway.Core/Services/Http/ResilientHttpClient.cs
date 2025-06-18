using Gateway.Core.Resilience;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Core.Services.Http
{
    public class ResilientHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly CircuitBreakerPolicyProvider _policyProvider;
        private readonly ILogger<ResilientHttpClient> _logger;
        private readonly string _serviceName;

        public ResilientHttpClient(
            HttpClient httpClient,
            CircuitBreakerPolicyProvider policyProvider,
            ILogger<ResilientHttpClient> logger,
            string serviceName)
        {
            _httpClient = httpClient;
            _policyProvider = policyProvider;
            _logger = logger;
            _serviceName = serviceName;
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            var policy = _policyProvider.GetOrCreatePolicy(_serviceName);

            try
            {
                return await policy.ExecuteAsync(async ct =>
                {
                    var response = await _httpClient.SendAsync(request, ct);

                    _logger.LogInformation(
                        "HTTP {Method} {Uri} responded with {StatusCode}",
                        request.Method, request.RequestUri, response.StatusCode);

                    return response;
                }, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TimeoutException)
            {
                _logger.LogError(ex,
                    "Failed HTTP request to {ServiceName}: {Method} {Uri}",
                    _serviceName, request.Method, request.RequestUri);
                throw;
            }
        }

        public async Task<T> GetJsonAsync<T>(
            string url,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: cancellationToken);
        }

        public async Task<T> PostJsonAsync<TRequest, T>(
            string url,
            TRequest value,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(value)
            };

            var response = await SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: cancellationToken);
        }
    }
}

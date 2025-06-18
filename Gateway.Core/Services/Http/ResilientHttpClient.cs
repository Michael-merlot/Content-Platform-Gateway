using Gateway.Core.Resilience;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }

        public async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken = default)
        {
            var pipeline = _policyProvider.GetOrCreatePolicy(_serviceName);

            try
            {
                return await pipeline.ExecuteAsync(async token =>
                {
                    var response = await _httpClient.SendAsync(request, token);

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

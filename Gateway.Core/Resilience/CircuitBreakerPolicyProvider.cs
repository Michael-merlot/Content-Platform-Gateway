using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Core.Resilience
{
    public class CircuitBreakerPolicyProvider
    {
        private readonly ILogger<CircuitBreakerPolicyProvider> _logger;

        // словарь для хранения политик по имени сервиса
        private readonly Dictionary<string, AsyncPolicyWrap<HttpResponseMessage>> _policies = new();

        public CircuitBreakerPolicyProvider(ILogger<CircuitBreakerPolicyProvider> logger)
        {
            _logger = logger;
        }

        public AsyncPolicyWrap<HttpResponseMessage> GetOrCreatePolicy(string serviceName)
        {
            if (_policies.TryGetValue(serviceName, out var existingPolicy))
            {
                return existingPolicy;
            }

            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .OrResult(r => !r.IsSuccessStatusCode) // учитываем также неуспешные HTTP ответы
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryAttempt} for {ServiceName} after {SleepDuration}s due to {Exception}",
                            retryAttempt, serviceName, timespan.TotalSeconds,
                            outcome.Exception?.Message ?? $"Status code {outcome.Result.StatusCode}");
                    });

            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .OrResult(r => (int)r.StatusCode >= 500) // учитываем только серверные ошибки
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan, context) =>
                    {
                        _logger.LogError(
                            "Circuit breaker for {ServiceName} tripped. Circuit will be open for {DurationOfBreak}s due to {Exception}",
                            serviceName, timespan.TotalSeconds,
                            outcome.Exception?.Message ?? $"Status code {outcome.Result.StatusCode}");
                    },
                    onReset: context =>
                    {
                        _logger.LogInformation("Circuit breaker for {ServiceName} reset and ready for use", serviceName);
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit breaker for {ServiceName} is half-open, testing requests", serviceName);
                    });

            var combinedPolicy = retryPolicy.WrapAsync(circuitBreakerPolicy);

            _policies[serviceName] = combinedPolicy;
            return combinedPolicy;
        }
    }
}

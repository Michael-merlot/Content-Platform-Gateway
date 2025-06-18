using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Core.Resilience
{
    public class CircuitBreakerPolicyProvider
    {
        private readonly ILogger<CircuitBreakerPolicyProvider> _logger;

        private readonly Dictionary<string, ResiliencePipeline<HttpResponseMessage>> _policies = new();

        public CircuitBreakerPolicyProvider(ILogger<CircuitBreakerPolicyProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ResiliencePipeline<HttpResponseMessage> GetOrCreatePolicy(string serviceName)
        {
            if (_policies.TryGetValue(serviceName, out var existingPolicy))
            {
                return existingPolicy;
            }

            var retryPredicate = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutException>()
                .HandleResult(response => !response.IsSuccessStatusCode);

            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = retryPredicate,
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    var attemptNumber = args.AttemptNumber;
                    var delay = args.RetryDelay;
                    var outcome = args.Outcome;

                    string errorMessage;
                    if (outcome.Exception != null)
                    {
                        errorMessage = outcome.Exception.Message;
                    }
                    else if (outcome.Result != null)
                    {
                        errorMessage = $"Status code {outcome.Result.StatusCode}";
                    }
                    else
                    {
                        errorMessage = "Unknown error";
                    }

                    _logger.LogWarning(
                        "Retry {RetryAttempt} for {ServiceName} after {SleepDuration}s due to {Exception}",
                        attemptNumber, serviceName, delay.TotalSeconds, errorMessage);

                    return ValueTask.CompletedTask;
                }
            };

            var circuitBreakerPredicate = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutException>()
                .HandleResult(response => (int)response.StatusCode >= 500);

            var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = circuitBreakerPredicate,
                FailureRatio = 0.5, 
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 5, 
                BreakDuration = TimeSpan.FromSeconds(30), 

                OnOpened = args =>
                {
                    _logger.LogError(
                        "Circuit breaker for {ServiceName} tripped. Circuit will be open for {DurationOfBreak}s",
                        serviceName, args.BreakDuration.TotalSeconds);

                    return ValueTask.CompletedTask;
                },

                OnClosed = _ =>
                {
                    _logger.LogInformation("Circuit breaker for {ServiceName} reset and ready for use", serviceName);
                    return ValueTask.CompletedTask;
                },

                OnHalfOpened = _ =>
                {
                    _logger.LogInformation("Circuit breaker for {ServiceName} is half-open, testing requests", serviceName);
                    return ValueTask.CompletedTask;
                }
            };

            var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(retryOptions)
                .AddCircuitBreaker(circuitBreakerOptions)
                .Build();

            _policies[serviceName] = pipeline;
            return pipeline;
        }
    }
}

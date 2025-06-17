using Gateway.Core.Interfaces.Clients;
using Gateway.Infrastructure.Clients;
using Gateway.Infrastructure.Logging;
using Gateway.Infrastructure.Persistence.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;
<<<<<<< HEAD
using Gateway.Core.Interfaces.Persistence;
using Microsoft.Extensions.Logging;
=======
using Polly.Timeout;
using Gateway.Core.Interfaces.Persistence;

>>>>>>> 781671d28f1477088a62376ca74b53f5fa26a8ca

namespace Gateway.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует все сервисы приложения в DI контейнере
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <returns>Коллекция сервисов с добавленными зависимостями</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClients(configuration);
            services.AddRedisStorage(configuration);
            services.AddLogging(configuration);

            return services;
        }

        /// <summary>
        /// Регистрирует HTTP клиенты для внешних сервисов
        /// </summary>
        private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ILaravelApiClient, LaravelApiClient>(client =>
            {
                var baseUrl = configuration["ExternalServices:LaravelApi"] ?? "http://localhost:8000";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandler(HttpClientPolicyHelpers.GetRetryPolicy())
              .AddPolicyHandler(HttpClientPolicyHelpers.GetCircuitBreakerPolicy());

            services.AddHttpClient<IAiServicesClient, AiServicesClient>(client =>
            {
                var baseUrl = configuration["ExternalServices:PythonAiServices"] ?? "http://localhost:5000";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddPolicyHandler(HttpClientPolicyHelpers.GetRetryPolicy())
              .AddPolicyHandler(HttpClientPolicyHelpers.GetCircuitBreakerPolicy());

            return services;
        }

        private static IServiceCollection AddRedisStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<ICacheRepository, RedisRepository>();

            return services;
        }

        private static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<SerilogConfigurator>();

            return services;
        }
    }

    public static class HttpClientPolicyHelpers
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Здесь логирование без использования сервисов из DI
                        // На данном этапе мы не можем использовать services
                        // Логирование можно выполнить через context.GetLogger() если передать его отдельно или вообще не логировать здесь
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        // Здесь мы также избегаем использования services
                    },
                    onReset: () =>
                    {
                        // И здесь
                    });
        }
    }

    public interface IRedisRepository
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}

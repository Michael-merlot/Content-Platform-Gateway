using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Interfaces.Persistence;
using Gateway.Infrastructure.Clients;
using Gateway.Infrastructure.Logging;
using Gateway.Infrastructure.Persistence.Mock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;
using System;
using System.Net.Http;

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
            services.AddStorageServices(configuration);
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

        private static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            var useRedisMock = true;

            if (useRedisMock)
            {
                services.AddScoped<ICacheRepository, Gateway.Infrastructure.Persistence.Mock.MockCacheRepository>();
            }
            else
            {
                var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;

                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(options));

                services.AddScoped<IRedisRepository, Gateway.Infrastructure.Persistence.Redis.RedisRepository>();
                services.AddScoped<ICacheRepository, Gateway.Infrastructure.Persistence.Redis.RedisRepository>();
            }

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
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
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

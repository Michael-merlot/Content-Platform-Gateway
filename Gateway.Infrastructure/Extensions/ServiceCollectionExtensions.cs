using Gateway.Core.Interfaces.Clients;
using Gateway.Infrastructure.Clients;
using Gateway.Infrastructure.Logging;
using Gateway.Infrastructure.Persistence.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;


namespace Gateway.Infrastructure.Extensions
{

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// регистрирует все сервисы приложения в DI контейнере
        /// </summary>
        /// <param name="services">коллекция сервисов</param>
        /// <param name="configuration">конфигурация приложения</param>
        /// <returns>коллекция сервисов с добавленными зависимостями</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClients(configuration);

            services.AddRedisStorage(configuration);

            services.AddLogging(configuration);

            return services;
        }

        /// <summary>
        /// регистрирует HTTP клиенты для внешних сервисов
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

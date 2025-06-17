using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Core.Exceptions;

namespace Gateway.Infrastructure.Extensions
{
    /// <summary>
    /// Методы расширения для HttpClient для работы с JSON API
    /// </summary>
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Выполняет GET запрос и десериализует ответ в указанный тип
        /// </summary>
        /// <typeparam name="T">Тип для десериализации</typeparam>
        /// <param name="httpClient">Экземпляр HttpClient</param>
        /// <param name="requestUri">URI запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Десериализованный объект</returns>
        public static async Task<T> GetFromJsonSafeAsync<T>(
            this HttpClient httpClient,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Для отладки можно логировать ответ сервера
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    return JsonSerializer.Deserialize<T>(content, DefaultSerializerOptions)!;
                }
                catch (JsonException ex)
                {
                    throw new JsonException($"Ошибка десериализации ответа от {requestUri}. " +
                        $"Ответ: {content}", ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Ошибка HTTP при вызове {requestUri}: {ex.Message}",
                    ex, ex.StatusCode);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ApiException($"Превышено время ожидания ответа от {requestUri}",
                    ex, 504, $"Timeout while calling {requestUri}");
            }
            catch (Exception ex) when (!(ex is ApiException || ex is JsonException))
            {
                throw new ApiException($"Неизвестная ошибка при вызове {requestUri}",
                    ex, 500, $"Unknown error while calling {requestUri}: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет POST запрос с JSON содержимым и десериализует ответ
        /// </summary>
        /// <typeparam name="TRequest">Тип тела запроса</typeparam>
        /// <typeparam name="TResponse">Тип ответа</typeparam>
        /// <param name="httpClient">Экземпляр HttpClient</param>
        /// <param name="requestUri">URI запроса</param>
        /// <param name="value">Тело запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Десериализованный ответ</returns>
        public static async Task<TResponse> PostAsJsonSafeAsync<TRequest, TResponse>(
            this HttpClient httpClient,
            string requestUri,
            TRequest value,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(value, DefaultSerializerOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Для отладки можно логировать ответ сервера
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(content, DefaultSerializerOptions)!;
                }
                catch (JsonException ex)
                {
                    throw new JsonException($"Ошибка десериализации ответа от {requestUri}. " +
                        $"Ответ: {content}", ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Ошибка HTTP при вызове {requestUri}: {ex.Message}",
                    ex, ex.StatusCode);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ApiException($"Превышено время ожидания ответа от {requestUri}",
                    ex, 504, $"Timeout while calling {requestUri}");
            }
            catch (Exception ex) when (!(ex is ApiException || ex is JsonException))
            {
                throw new ApiException($"Неизвестная ошибка при вызове {requestUri}",
                    ex, 500, $"Unknown error while calling {requestUri}: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет POST запрос с данными формы и десериализует ответ
        /// </summary>
        /// <typeparam name="TResponse">Тип ответа</typeparam>
        /// <param name="httpClient">Экземпляр HttpClient</param>
        /// <param name="requestUri">URI запроса</param>
        /// <param name="content">Содержимое формы</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Десериализованный ответ</returns>
        public static async Task<TResponse> PostMultipartFormDataSafeAsync<TResponse>(
            this HttpClient httpClient,
            string requestUri,
            MultipartFormDataContent content,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = content;

                using var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Для отладки можно логировать ответ сервера
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(responseContent, DefaultSerializerOptions)!;
                }
                catch (JsonException ex)
                {
                    throw new JsonException($"Ошибка десериализации ответа от {requestUri}. " +
                        $"Ответ: {responseContent}", ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Ошибка HTTP при вызове {requestUri}: {ex.Message}",
                    ex, ex.StatusCode);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ApiException($"Превышено время ожидания ответа от {requestUri}",
                    ex, 504, $"Timeout while calling {requestUri}");
            }
            catch (Exception ex) when (!(ex is ApiException || ex is JsonException))
            {
                throw new ApiException($"Неизвестная ошибка при вызове {requestUri}",
                    ex, 500, $"Unknown error while calling {requestUri}: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет PUT запрос с JSON содержимым и десериализует ответ
        /// </summary>
        /// <typeparam name="TRequest">Тип тела запроса</typeparam>
        /// <typeparam name="TResponse">Тип ответа</typeparam>
        /// <param name="httpClient">Экземпляр HttpClient</param>
        /// <param name="requestUri">URI запроса</param>
        /// <param name="value">Тело запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Десериализованный ответ</returns>
        public static async Task<TResponse> PutAsJsonSafeAsync<TRequest, TResponse>(
            this HttpClient httpClient,
            string requestUri,
            TRequest value,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(value, DefaultSerializerOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Для отладки можно логировать ответ сервера
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(content, DefaultSerializerOptions)!;
                }
                catch (JsonException ex)
                {
                    throw new JsonException($"Ошибка десериализации ответа от {requestUri}. " +
                        $"Ответ: {content}", ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Ошибка HTTP при вызове {requestUri}: {ex.Message}",
                    ex, ex.StatusCode);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ApiException($"Превышено время ожидания ответа от {requestUri}",
                    ex, 504, $"Timeout while calling {requestUri}");
            }
            catch (Exception ex) when (!(ex is ApiException || ex is JsonException))
            {
                throw new ApiException($"Неизвестная ошибка при вызове {requestUri}",
                    ex, 500, $"Unknown error while calling {requestUri}: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет DELETE запрос и десериализует ответ
        /// </summary>
        /// <typeparam name="TResponse">Тип ответа</typeparam>
        /// <param name="httpClient">Экземпляр HttpClient</param>
        /// <param name="requestUri">URI запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Десериализованный ответ</returns>
        public static async Task<TResponse> DeleteWithResponseSafeAsync<TResponse>(
            this HttpClient httpClient,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Для отладки можно логировать ответ сервера
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    return JsonSerializer.Deserialize<TResponse>(content, DefaultSerializerOptions)!;
                }
                catch (JsonException ex)
                {
                    throw new JsonException($"Ошибка десериализации ответа от {requestUri}. " +
                        $"Ответ: {content}", ex);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Ошибка HTTP при вызове {requestUri}: {ex.Message}",
                    ex, ex.StatusCode);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ApiException($"Превышено время ожидания ответа от {requestUri}",
                    ex, 504, $"Timeout while calling {requestUri}");
            }
            catch (Exception ex) when (!(ex is ApiException || ex is JsonException))
            {
                throw new ApiException($"Неизвестная ошибка при вызове {requestUri}",
                    ex, 500, $"Unknown error while calling {requestUri}: {ex.Message}");
            }
        }
    }
}

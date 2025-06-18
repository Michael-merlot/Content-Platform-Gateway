using Gateway.Core.Exceptions;
using Gateway.Core.Interfaces.Clients;
using Gateway.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Clients
{
    /// <summary>
    /// Реализация клиента для взаимодействия с Python AI сервисами
    /// </summary>
    public class AiServicesClient : IAiServicesClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiServicesClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AiServicesClient(HttpClient httpClient, ILogger<AiServicesClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <inheritdoc/>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check for AI services failed");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ContentRecommendation>> GetPersonalizedRecommendationsAsync(
            int userId,
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new
                {
                    UserId = userId,
                    Count = count
                };

                return await _httpClient.PostAsJsonSafeAsync<object, List<ContentRecommendation>>(
                    "/api/v1/recommendations/personalized",
                    request,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving personalized recommendations for userId: {UserId}", userId);
                throw HandleApiException(ex, "персонализированных рекомендаций");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving personalized recommendations");
                throw new ApiException(
                    "Ошибка при получении персонализированных рекомендаций",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ContentRecommendation>> GetNonPersonalizedRecommendationsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new
                {
                    Count = count
                };

                return await _httpClient.PostAsJsonSafeAsync<object, List<ContentRecommendation>>(
                    "/api/v1/recommendations/non-personalized",
                    request,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving non-personalized recommendations");
                throw HandleApiException(ex, "общих рекомендаций");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving non-personalized recommendations");
                throw new ApiException(
                    "Ошибка при получении общих рекомендаций",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ContentRecommendation>> GetTrendingContentAsync(
            string? contentType = null,
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string url = "/api/v1/trending";
                if (!string.IsNullOrEmpty(contentType))
                {
                    url += $"?contentType={Uri.EscapeDataString(contentType)}";
                }
                url += string.IsNullOrEmpty(contentType) ? $"?count={count}" : $"&count={count}";

                return await _httpClient.GetFromJsonSafeAsync<List<ContentRecommendation>>(url, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving trending content");
                throw HandleApiException(ex, "трендового контента");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving trending content");
                throw new ApiException(
                    "Ошибка при получении трендового контента",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task<VoiceQueryResult> ProcessVoiceQueryAsync(
            byte[] audioData,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var content = new MultipartFormDataContent();

                var audioContent = new ByteArrayContent(audioData);
                audioContent.Headers.Add("Content-Type", "audio/wav");
                content.Add(audioContent, "audio", "query.wav");

                if (!string.IsNullOrEmpty(userId))
                {
                    content.Add(new StringContent(userId), "userId");
                }

                return await _httpClient.PostMultipartFormDataSafeAsync<VoiceQueryResult>(
                    "/api/v1/naomi/voice",
                    content,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error processing voice query");
                throw HandleApiException(ex, "обработки голосового запроса");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing voice query");
                throw new ApiException(
                    "Ошибка при обработке голосового запроса",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task<VoiceQueryResult> ProcessTextQueryAsync(
            string query,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new
                {
                    Query = query,
                    UserId = userId
                };

                return await _httpClient.PostAsJsonSafeAsync<object, VoiceQueryResult>(
                    "/api/v1/naomi/text",
                    request,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error processing text query: {Query}", query);
                throw HandleApiException(ex, "обработки текстового запроса");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing text query");
                throw new ApiException(
                    "Ошибка при обработке текстового запроса",
                    ex);
            }
        }

        /// <summary>
        /// Преобразует HTTP исключение в ApiException с контекстным сообщением
        /// </summary>
        private ApiException HandleApiException(HttpRequestException ex, string context)
        {
            int statusCode = (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError);

            string userMessage = statusCode switch
            {
                404 => $"Не удалось найти данные для {context}.",
                401 or 403 => $"У вас нет доступа к {context}.",
                429 => $"Слишком много запросов для получения {context}. Попробуйте позже.",
                >= 500 => $"Сервис временно недоступен. Не удалось получить данные {context}.",
                _ => $"Ошибка при получении {context}: {ex.Message}"
            };

            return new ApiException(userMessage, ex, statusCode);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gateway.Core.Interfaces.Clients
{
    /// <summary>
    /// Клиент для взаимодействия с Python AI сервисами
    /// </summary>
    public interface IAiServicesClient
    {
        /// <summary>Проверяет доступность AI сервисов</summary>
        Task<bool> IsHealthyAsync();

        /// <summary>
        /// Получает персонализированные рекомендации для пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="count">Количество рекомендаций</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список рекомендуемого контента</returns>
        Task<IEnumerable<ContentRecommendation>> GetPersonalizedRecommendationsAsync(
            string userId,
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает неперсонализированные рекомендации
        /// </summary>
        /// <param name="count">Количество рекомендаций</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список рекомендуемого контента</returns>
        Task<IEnumerable<ContentRecommendation>> GetNonPersonalizedRecommendationsAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает трендовые элементы контента
        /// </summary>
        /// <param name="contentType">Тип контента (опционально)</param>
        /// <param name="count">Количество элементов</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список трендовых элементов</returns>
        Task<IEnumerable<ContentRecommendation>> GetTrendingContentAsync(
            string? contentType = null,
            int count = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Отправляет голосовой запрос в Naomi AI
        /// </summary>
        /// <param name="audioData">Байты аудио данных</param>
        /// <param name="userId">ID пользователя (опционально)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обработки голосового запроса</returns>
        Task<VoiceQueryResult> ProcessVoiceQueryAsync(
            byte[] audioData,
            string? userId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Отправляет текстовый запрос в Naomi AI
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="userId">ID пользователя (опционально)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обработки текстового запроса</returns>
        Task<VoiceQueryResult> ProcessTextQueryAsync(
            string query,
            string? userId = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Модель для рекомендуемого контента
    /// </summary>
    public class ContentRecommendation
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
    }

    /// <summary>
    /// Результат обработки голосового запроса
    /// </summary>
    public class VoiceQueryResult
    {
        public string RecognizedText { get; set; } = string.Empty;
        public string ResponseText { get; set; } = string.Empty;
        public IEnumerable<ContentRecommendation>? RelatedContent { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}

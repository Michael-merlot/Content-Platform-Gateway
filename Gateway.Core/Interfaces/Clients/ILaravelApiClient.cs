using Gateway.Core.Models.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Clients
{
    /// <summary>
    /// Клиент для взаимодействия с PHP Laravel API
    /// </summary>
    public interface ILaravelApiClient
    {
        /// <summary>Проверяет доступность Laravel API</summary>
        Task<bool> IsHealthyAsync();

        /// <summary>Аутентифицирует пользователя</summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат аутентификации</returns>
        Task<AuthenticationResult<LoginResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

        /// <summary>Проверяет MFA код</summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="code">Код для проверки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат проверки</returns>
        Task<AuthenticationResult<AuthenticatedTokenSession>> VerifyMultiFactorAsync(
            int userId,
            string code,
            CancellationToken cancellationToken = default);

        /// <summary>Обновляет сессию</summary>
        /// <param name="refreshToken">Токен обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обновления токена</returns>
        Task<AuthenticationResult<AuthenticatedTokenSession>> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);

        /// <summary>Выполняет выход пользователя</summary>
        /// <param name="accessToken">Токен доступа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат выполнения операции</returns>
        Task<AuthenticationResult> LogoutAsync(
            string accessToken,
            CancellationToken cancellationToken = default);

        /// <summary>Получает информацию о пользователе</summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о пользователе</returns>
        Task<UserProfileResponse?> GetUserProfileAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Модель для ответа с профилем пользователя
    /// </summary>
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}

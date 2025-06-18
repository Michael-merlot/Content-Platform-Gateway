using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Core.Exceptions;
using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Models.Auth;
using Gateway.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.Clients
{
    /// <summary>
    /// Реализация клиента для взаимодействия с PHP Laravel API
    /// </summary>
    public class LaravelApiClient : ILaravelApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LaravelApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public LaravelApiClient(HttpClient httpClient, ILogger<LaravelApiClient> logger)
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
                var response = await _httpClient.GetAsync("/api/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check for Laravel API failed");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult<LoginResult>> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new
                {
                    Email = email,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonSafeAsync<object, LoginApiResponse>(
                    "/api/v1/auth/login",
                    request,
                    cancellationToken);

                if (response.MfaRequired)
                {
                    return new AuthResult<LoginResult>(
                        new LoginResult(
                            MfaRequired: true,
                            AuthTokenSession: null,
                            MfaVerificationRequiredMetadata: new MfaVerificationMetadata(response.UserId)),
                        AuthError.None,
                        null);
                }
                else
                {
                    return new AuthResult<LoginResult>(
                        new LoginResult(
                            MfaRequired: false,
                            AuthTokenSession: new AuthTokenSession(
                                response.AccessToken,
                                response.RefreshToken,
                                response.ExpiresIn,
                                response.TokenType),
                            MfaVerificationRequiredMetadata: null),
                        AuthError.None,
                        null);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during login request");
                return HandleHttpException<LoginResult>(ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing login response");
                return new AuthResult<LoginResult>(
                    null,
                    AuthError.ServerError,
                    "Ошибка обработки ответа сервера");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return new AuthResult<LoginResult>(
                    null,
                    AuthError.ServerError,
                    "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult<AuthTokenSession>> VerifyMultiFactorAsync(
            int userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new
                {
                    UserId = userId,
                    Code = code
                };

                var response = await _httpClient.PostAsJsonSafeAsync<object, AuthApiResponse>(
                    "/api/v1/auth/verify-mfa",
                    request,
                    cancellationToken);

                return new AuthResult<AuthTokenSession>(
                    new AuthTokenSession(
                        response.AccessToken,
                        response.RefreshToken,
                        response.ExpiresIn,
                        response.TokenType),
                    AuthError.None,
                    null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during MFA verification request");
                return HandleHttpException<AuthTokenSession>(ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing MFA verification response");
                return new AuthResult<AuthTokenSession>(
                    null,
                    AuthError.ServerError,
                    "Ошибка обработки ответа сервера");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during MFA verification");
                return new AuthResult<AuthTokenSession>(
                    null,
                    AuthError.ServerError,
                    "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult<AuthTokenSession>> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new
                {
                    RefreshToken = refreshToken
                };

                var response = await _httpClient.PostAsJsonSafeAsync<object, AuthApiResponse>(
                    "/api/v1/auth/refresh",
                    request,
                    cancellationToken);

                return new AuthResult<AuthTokenSession>(
                    new AuthTokenSession(
                        response.AccessToken,
                        response.RefreshToken,
                        response.ExpiresIn,
                        response.TokenType),
                    AuthError.None,
                    null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during token refresh request");
                return HandleHttpException<AuthTokenSession>(ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing refresh token response");
                return new AuthResult<AuthTokenSession>(
                    null,
                    AuthError.ServerError,
                    "Ошибка обработки ответа сервера");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return new AuthResult<AuthTokenSession>(
                    null,
                    AuthError.ServerError,
                    "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult> LogoutAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return new AuthResult(AuthError.None, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error during logout request");
                return HandleHttpException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout");
                return new AuthResult(AuthError.ServerError, "Произошла непредвиденная ошибка");
            }
        }

        /// <inheritdoc/>
        public async Task<UserProfileResponse?> GetUserProfileAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpClient.GetFromJsonSafeAsync<UserProfileResponse>(
                    $"/api/v1/users/{userId}/profile",
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for userId: {UserId}", userId);
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw new ApiException(
                    $"Ошибка при получении профиля пользователя: {ex.Message}",
                    ex,
                    (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user profile");
                throw new ApiException(
                    "Ошибка при получении профиля пользователя",
                    ex);
            }
        }

        #region Helper methods

        private AuthResult HandleHttpException(HttpRequestException ex)
        {
            var error = ex.StatusCode switch
            {
                HttpStatusCode.BadRequest => AuthError.InvalidRequest,
                HttpStatusCode.Unauthorized => AuthError.InvalidClient,
                HttpStatusCode.Forbidden => AuthError.Forbidden,
                HttpStatusCode.NotFound => AuthError.NotFound,
                HttpStatusCode.BadGateway => AuthError.ServerError,
                _ => AuthError.NetworkError
            };

            return new AuthResult(error, ex.Message);
        }

        private AuthResult<T> HandleHttpException<T>(HttpRequestException ex)
        {
            var error = ex.StatusCode switch
            {
                HttpStatusCode.BadRequest => AuthError.InvalidRequest,
                HttpStatusCode.Unauthorized => AuthError.InvalidClient,
                HttpStatusCode.Forbidden => AuthError.Forbidden,
                HttpStatusCode.NotFound => AuthError.NotFound,
                HttpStatusCode.BadGateway => AuthError.ServerError,
                _ => AuthError.NetworkError
            };

            return new AuthResult<T>(default, error, ex.Message);
        }

        #endregion

        #region API Response Models

        private class LoginApiResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public int ExpiresIn { get; set; }
            public string TokenType { get; set; } = "Bearer";
            public bool MfaRequired { get; set; }
            public int UserId { get; set; }
        }

        private class AuthApiResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public int ExpiresIn { get; set; }
            public string TokenType { get; set; } = "Bearer";
        }

        #endregion
    }
}

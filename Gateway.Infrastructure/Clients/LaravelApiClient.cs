using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Models.Auth;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Infrastructure.Clients
{
    public class LaravelApiClient : ILaravelApiClient
    {
        private readonly HttpClient _httpClient;

        public LaravelApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Authenticates the user</summary>
        /// <param name="email">User e-mail</param>
        /// <param name="password">User password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication result</returns>
        public async Task<AuthResult<AuthTokenSession>>
            LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        /// <summary>Refreshes the session</summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refresh token result</returns>
        public async Task<AuthResult<AuthTokenSession>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        /// <summary>Logs the user out</summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Whether the log out completed successfully</returns>
        public async Task<AuthResult> LogoutAsync(string accessToken, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}

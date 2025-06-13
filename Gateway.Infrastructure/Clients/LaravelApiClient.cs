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

        /// <inheritdoc/>
        public async Task<AuthResult<LoginResult>>
            LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task<AuthResult<AuthTokenSession>> VerifyMultiFactorAsync(string userId, string code,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task<AuthResult<AuthTokenSession>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task<AuthResult> LogoutAsync(string accessToken, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}

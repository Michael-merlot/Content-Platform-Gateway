using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Clients
{
    public interface ILaravelApiClient
    {
        Task<bool> IsHealthyAsync();

        /// <summary>Authenticates the user</summary>
        /// <param name="email">User e-mail</param>
        /// <param name="password">User password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication result</returns>
        Task<AuthResult<AuthTokenSession>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

        /// <summary>Refreshes the session</summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refresh token result</returns>
        Task<AuthResult<AuthTokenSession>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>Logs the user out</summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Whether the log out completed successfully</returns>
        Task<AuthResult> LogoutAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}

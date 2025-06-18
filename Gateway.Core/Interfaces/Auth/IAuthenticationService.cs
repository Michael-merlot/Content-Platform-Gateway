using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Service that manages authentication.</summary>
public interface IAuthenticationService
{
    /// <summary>Authenticates the user.</summary>
    /// <param name="email">User e-mail.</param>
    /// <param name="password">User password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication result.</returns>
    Task<AuthResult<LoginResult>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>Verifies MFA code.</summary>
    /// <param name="userId">The unique identifier of the user to verify.</param>
    /// <param name="code">The code to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication result.</returns>
    Task<AuthResult<AuthTokenSession>> VerifyMultiFactorAsync(int userId, string code, CancellationToken cancellationToken = default);

    /// <summary>Refreshes the session.</summary>
    /// <param name="refreshToken">Refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Refresh token result.</returns>
    Task<AuthResult<AuthTokenSession>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Logs the user out.</summary>
    /// <param name="accessToken">Access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the log out completed successfully.</returns>
    Task<AuthResult> LogoutAsync(string accessToken, CancellationToken cancellationToken = default);
}

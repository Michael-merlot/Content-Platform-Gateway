using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Services.Auth;

/// <inheritdoc/>
internal class AuthenticationService : IAuthenticationService
{
    private readonly ILaravelApiClient _apiClient;

    public AuthenticationService(ILaravelApiClient apiClient) =>
        _apiClient = apiClient;

    /// <inheritdoc/>
    public async Task<AuthResult<AuthTokenSession>>
        LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
        await _apiClient.LoginAsync(email, password, cancellationToken);

    /// <inheritdoc/>
    public async Task<AuthResult<AuthTokenSession>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        await _apiClient.RefreshAsync(refreshToken, cancellationToken);

    /// <inheritdoc/>
    public async Task<AuthResult> LogoutAsync(string accessToken, CancellationToken cancellationToken = default) =>
        await _apiClient.LogoutAsync(accessToken, cancellationToken);
}

namespace Gateway.Core.Models.Auth;

/// <summary>Represents the session of token pair and their metadata.</summary>
/// <param name="AccessToken">Access token.</param>
/// <param name="RefreshToken">Refresh token.</param>
/// <param name="ExpiresIn">The time the access token expires in.</param>
/// <param name="TokenType">Token type.</param>
public sealed record AuthenticatedTokenSession(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType
);

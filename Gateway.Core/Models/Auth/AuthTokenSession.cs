namespace Gateway.Core.Models.Auth;

/// <summary>
/// The session of token pair and their metadata
/// </summary>
/// <param name="AccessToken">Access token</param>
/// <param name="RefreshToken">Refresh token</param>
/// <param name="ExpiresIn">The time it expires in</param>
/// <param name="TokenType">Token type</param>
public sealed record AuthTokenSession(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType
);

namespace Gateway.Api.Models.Auth;

/// <summary>Represents an authentication response.</summary>
/// <param name="AccessToken">Access token.</param>
/// <param name="RefreshToken">Refresh token.</param>
/// <param name="ExpiresIn">The time access token expires in.</param>
/// <param name="TokenType">Token type.</param>
/// <param name="MfaRequired">Whether or not MFA is required (always <c>false</c>).</param>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    bool MfaRequired = false
) : LoginResponse;

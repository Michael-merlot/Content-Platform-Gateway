namespace Gateway.Core.Models.Auth;

/// <summary>The result of the login</summary>
/// <param name="MfaRequired">Whether or not MFA verification is required</param>
/// <param name="AuthTokenSession">Not <c>null</c> when <paramref name="MfaRequired"/> is <c>false</c></param>
/// <param name="MfaVerificationRequiredMetadata">Not <c>null</c> when <paramref name="MfaRequired"/> is <c>true</c></param>
public sealed record LoginResult(
    bool MfaRequired,
    AuthTokenSession? AuthTokenSession,
    MfaVerificationMetadata? MfaVerificationRequiredMetadata
);

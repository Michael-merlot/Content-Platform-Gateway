namespace Gateway.Core.Models.Auth;

/// <summary>Represents the result of the login.</summary>
public abstract record LoginResult;

/// <summary>Represents the succeeded login result.</summary>
/// <param name="TokenSession">The token session received in the result of the login</param>
public sealed record LoginSucceeded(AuthenticatedTokenSession TokenSession) : LoginResult;

/// <summary>Represents the login result which requires MFA verification.</summary>
/// <param name="Metadata">MFA verification metadata.</param>
public sealed record MfaVerificationRequired(MfaVerificationMetadata Metadata) : LoginResult;

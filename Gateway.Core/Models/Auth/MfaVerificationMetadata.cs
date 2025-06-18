namespace Gateway.Core.Models.Auth;

/// <summary>The metadata needed to verify MFA for a <paramref name="UserId"/></summary>
/// <param name="UserId">The ID of a user who needs to be verified</param>
public sealed record MfaVerificationMetadata(int UserId);

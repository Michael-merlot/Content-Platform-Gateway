namespace Gateway.Core.Models.Auth;

/// <summary>Represents the metadata needed to verify MFA for a user.</summary>
/// <param name="UserId">The unique identifier of the user to verify.</param>
public sealed record MfaVerificationMetadata(int UserId);

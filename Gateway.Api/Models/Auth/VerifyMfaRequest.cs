using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a MFA verification request.</summary>
/// <param name="UserId">The unique identifier of the user to verify.</param>
/// <param name="Code">MFA code.</param>
public sealed record VerifyMfaRequest(
    int UserId,
    [MaxLength(10)] string Code
);

using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>MFA verification request</summary>
/// <param name="UserId">The ID of a user who needs to be verified</param>
/// <param name="Code">MFA code</param>
public sealed record VerifyMfaRequest(
    int UserId,
    [MaxLength(10)] string Code
);

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a response requiring MFA.</summary>
/// <param name="UserId">The unique identifier of the user to verify.</param>
/// <param name="Error">An error message.</param>
/// <param name="MfaRequired">Whether or not MFA is required (always <c>true</c>).</param>
public sealed record MfaRequiredResponse(
    int UserId,
    string Error = "MFA verification is required",
    bool MfaRequired = true
) : LoginResponse;

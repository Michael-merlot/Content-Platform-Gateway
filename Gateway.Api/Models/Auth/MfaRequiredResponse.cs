namespace Gateway.Api.Models.Auth;

/// <summary>Response requiring MFA</summary>
/// <param name="UserId">The ID of a user who needs to be verified</param>
/// <param name="Error">An error message</param>
/// <param name="MfaRequired">Whether or not MFA is required (always <c>true</c>)</param>
public sealed record MfaRequiredResponse(
    int UserId,
    string Error = "MFA verification is required",
    bool MfaRequired = true
) : LoginResponse;

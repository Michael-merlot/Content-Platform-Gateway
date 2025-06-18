namespace Gateway.Api.Models.Auth;

/// <summary>Represents a login response that differs on whether or not MFA is required or not.</summary>
public abstract record LoginResponse
{
    /// <summary>Whether or not MFA is required.</summary>
    public abstract bool MfaRequired { get; init; }
}

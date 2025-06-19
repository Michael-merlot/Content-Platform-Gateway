namespace Gateway.Core.Models.Auth;

/// <summary>Represents an authentication result.</summary>
/// <param name="Data">The data returned on success.</param>
/// <param name="Error">The error.</param>
/// <param name="ErrorDescription">The error description, if any.</param>
/// <typeparam name="T">The type of returned data.</typeparam>
public sealed record AuthenticationResult<T>(
    T? Data,
    AuthenticationError Error,
    string? ErrorDescription
) : AuthenticationResult(Error, ErrorDescription);

/// <summary>Represents an authentication result.</summary>
/// <param name="Error">The error.</param>
/// <param name="ErrorDescription">The error description, if any.</param>
public record AuthenticationResult(
    AuthenticationError Error,
    string? ErrorDescription
)
{
    /// <summary>Whether or not the operation succeeded.</summary>
    public bool IsSuccess => Error == AuthenticationError.None;
}

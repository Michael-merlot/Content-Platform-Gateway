namespace Gateway.Core.Models.Auth;

/// <summary>Represents an authentication error.</summary>
public enum AuthenticationError
{
    None,
    InvalidRequest,
    InvalidClient,
    InvalidGrant,
    UnsupportedGrant,
    Forbidden,
    NotFound,
    ServerError,
    NetworkError
}

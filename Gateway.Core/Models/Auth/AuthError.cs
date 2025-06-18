namespace Gateway.Core.Models.Auth;

/// <summary>Represents an authentication error.</summary>
public enum AuthError
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

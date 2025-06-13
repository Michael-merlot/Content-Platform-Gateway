namespace Gateway.Core.Models.Auth;

/// <summary>Authentication error</summary>
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

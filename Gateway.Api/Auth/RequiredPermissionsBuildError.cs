namespace Gateway.Api.Auth;

/// <summary>Represents an error that can occur while building required permissions collection.</summary>
public enum RequiredPermissionsBuildError
{
    None,
    InappropriateContext,
    EndpointNotFound
}

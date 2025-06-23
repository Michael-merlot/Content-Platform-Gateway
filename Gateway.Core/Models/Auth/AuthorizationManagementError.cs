namespace Gateway.Core.Models.Auth;

/// <summary>Represents an authorization management error.</summary>
public enum AuthorizationManagementError
{
    None,
    Unknown,
    BadRequest,
    EntityAlreadyExists,
    AnyEntityNotFound,
    RoleNotFound,
    PermissionNotFound,
    EndpointNotFound,
    UserNotFound
}

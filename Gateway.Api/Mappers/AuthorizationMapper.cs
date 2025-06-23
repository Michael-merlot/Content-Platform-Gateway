using Gateway.Api.Models.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Mvc;

using Endpoint = Gateway.Core.Models.Auth.Endpoint;

namespace Gateway.Api.Mappers;

internal static class AuthorizationMapper
{
    /// <summary>
    /// Constructs <see cref="IActionResult"/> with problem details from the <see cref="AuthorizationManagementError"/>.
    /// </summary>
    /// <param name="error">The <see cref="AuthorizationManagementError"/> to construct from.</param>
    /// <param name="controller">The controller from which the problem details will be constructed.</param>
    /// <returns>Problem details action result.</returns>
    public static IActionResult ToProblemDetails(this AuthorizationManagementError error, ControllerBase controller) =>
        controller.Problem(statusCode: error.ToHttpStatus(),
            title: error.ToString());

    /// <summary>Maps the role to the admin DTO.</summary>
    /// <param name="role">The role to map.</param>
    /// <returns>Mapped role DTO.</returns>
    public static RoleAdminDto ToAdminDto(this Role role) =>
        new(role.Id, role.Name, role.IsAdmin);

    /// <summary>Maps the role collection to the admin DTO.</summary>
    /// <param name="roles">The role collection to map.</param>
    /// <returns>Mapped role collection DTO.</returns>
    public static RoleCollectionAdminResponse ToAdminDto(this IEnumerable<Role> roles) =>
        new(roles.Select(ToAdminDto));

    /// <summary>Maps the permission to the DTO.</summary>
    /// <param name="permission">The permission to map.</param>
    /// <returns>Mapped permission DTO.</returns>
    public static PermissionDto ToDto(this Permission permission) =>
        new(permission.Id, permission.Name);

    /// <summary>Maps the permission collection to the admin DTO.</summary>
    /// <param name="permissions">The permission collection to map.</param>
    /// <returns>Mapped permission collection DTO.</returns>
    public static PermissionCollectionResponse ToDto(this IEnumerable<Permission> permissions) =>
        new(permissions.Select(ToDto));

    /// <summary>Maps the endpoint to the DTO.</summary>
    /// <param name="endpoint">The endpoint to map.</param>
    /// <returns>Mapped endpoint DTO.</returns>
    public static EndpointDto ToDto(this Endpoint endpoint) =>
        new(endpoint.Id, endpoint.Controller, endpoint.Action, endpoint.HttpMethod);

    /// <summary>Maps the endpoint collection to the admin DTO.</summary>
    /// <param name="endpoints">The endpoint collection to map.</param>
    /// <returns>Mapped endpoint collection DTO.</returns>
    public static EndpointCollectionResponse ToDto(this IEnumerable<Endpoint> endpoints) =>
        new(endpoints.Select(x => new EndpointDto(x.Id, x.Controller, x.Action, x.HttpMethod)));

    /// <summary>Maps the framework endpoint collection to the admin DTO.</summary>
    /// <param name="frameworkEndpoints">The framework endpoint collection to map.</param>
    /// <returns>Mapped framework endpoint collection DTO.</returns>
    public static FrameworkEndpointCollectionResponse ToDto(this IEnumerable<FrameworkEndpointDto> frameworkEndpoints) =>
        new(frameworkEndpoints);

    /// <summary>Maps <see cref="AuthorizationManagementError"/> to <see cref="int"/> HTTP status code.</summary>
    /// <param name="error">The <see cref="AuthorizationManagementError"/> to map.</param>
    /// <returns>HTTP status code.</returns>
    private static int ToHttpStatus(this AuthorizationManagementError error) =>
        error switch
        {
            AuthorizationManagementError.None => StatusCodes.Status200OK,
            AuthorizationManagementError.Unknown => StatusCodes.Status500InternalServerError,
            AuthorizationManagementError.BadRequest => StatusCodes.Status400BadRequest,
            AuthorizationManagementError.EntityAlreadyExists => StatusCodes.Status409Conflict,
            AuthorizationManagementError.AnyEntityNotFound => StatusCodes.Status404NotFound,
            AuthorizationManagementError.RoleNotFound => StatusCodes.Status404NotFound,
            AuthorizationManagementError.PermissionNotFound => StatusCodes.Status404NotFound,
            AuthorizationManagementError.EndpointNotFound => StatusCodes.Status404NotFound,
            AuthorizationManagementError.UserNotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
}

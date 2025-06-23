using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>The authorization management service.</summary>
public interface IAuthorizationManagementService
{
    /// <summary>Gets the role</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The role or an error.</returns>
    Task<Result<Role, AuthorizationManagementError>> GetRoleAsync(long roleId);

    /// <summary>Gets all roles.</summary>
    /// <returns>The collection of roles or an error.</returns>
    Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetRolesAsync();

    /// <summary>Creates a role.</summary>
    /// <param name="name">The name of a role.</param>
    /// <returns>The created role or an error.</returns>
    Task<Result<Role, AuthorizationManagementError>> CreateRoleAsync(string name);

    /// <summary>Deletes the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> DeleteRoleAsync(long roleId);

    /// <summary>Gets role permissions.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The collection of role permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetRolePermissionsAsync(long roleId);

    /// <summary>Adds pemission to the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> AddPermissionToRoleAsync(long roleId, long permissionId);

    /// <summary>Removes permission from the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> RemovePermissionFromRoleAsync(long roleId, long permissionId);

    /// <summary>Gets user roles.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of roles or an error.</returns>
    Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetUserRolesAsync(int userId);

    /// <summary>Gets user permissions.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetUserPermissionsAsync(int userId);

    /// <summary>Adds role to the user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> AddRoleToUserAsync(int userId, long roleId);

    /// <summary>Removes role from the user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> RemoveRoleFromUserAsync(int userId, long roleId);

    /// <summary>Gets the permission.</summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>The permission or an error.</returns>
    Task<Result<Permission, AuthorizationManagementError>> GetPermissionAsync(long permissionId);

    /// <summary>Gets all permissions.</summary>
    /// <returns>The collection of all permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetPermissionsAsync();

    /// <summary>Creates a permission.</summary>
    /// <param name="name">The name of a permission.</param>
    /// <param name="description">The description of a permission.</param>
    /// <returns>The created permission or an error.</returns>
    Task<Result<Permission, AuthorizationManagementError>> CreatePermissionAsync(string name, string? description);

    /// <summary>Deletes the permission.</summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> DeletePermissionAsync(long permissionId);

    /// <summary>Gets the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>The endpoint or an error.</returns>
    Task<Result<Endpoint, AuthorizationManagementError>> GetEndpointAsync(long endpointId);

    /// <summary>Gets all endpoints.</summary>
    /// <returns>The collection of all endpoints or an error.</returns>
    Task<Result<IEnumerable<Endpoint>, AuthorizationManagementError>> GetEndpointsAsync();

    /// <summary>Creates an endpoint.</summary>
    /// <param name="controller">The controller of an endpoint.</param>
    /// <param name="action">The action of an endpoint.</param>
    /// <param name="httpMethod">The HTTP method of an endpoint.</param>
    /// <returns>The created endpoint or an error.</returns>
    Task<Result<Endpoint, AuthorizationManagementError>> CreateEndpointAsync(string controller, string action, string httpMethod);

    /// <summary>Deletes the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> DeleteEndpointAsync(long endpointId);

    /// <summary>Gets endpoint permission requirements.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>The collection of permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(long endpointId);

    /// <summary>Gets endpoint permission requirements.</summary>
    /// <param name="controller">The controller of the endpoint.</param>
    /// <param name="action">The action of the endpoint.</param>
    /// <param name="httpMethod">The HTTP method of the endpoint.</param>
    /// <returns>The collection of permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(string controller,
        string action, string httpMethod);

    /// <summary>Adds permission requirement to the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> AddPermissionRequirementToEndpointAsync(long endpointId, long permissionId);

    /// <summary>Removes permission requirement from the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> RemovePermissionRequirementFromEndpointAsync(long endpointId, long permissionId);
}

using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to user roles.</summary>
public interface IRoleRepository
{
    /// <summary>Gets the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The role or an error.</returns>
    Task<Result<Role, AuthorizationManagementError>> GetRoleAsync(long roleId);

    /// <summary>Gets all roles.</summary>
    /// <returns>The collection of all roles or an error.</returns>
    Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetRolesAsync();

    /// <summary>Creates the role.</summary>
    /// <param name="name">The name of the role.</param>
    /// <returns>The created role or an error.</returns>
    Task<Result<Role, AuthorizationManagementError>> CreateRoleAsync(string name);

    /// <summary>Deletes the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> DeleteRoleAsync(long roleId);

    /// <summary>Gets permissions of the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The collection of role permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetRolePermissionsAsync(long roleId);

    /// <summary>Assigns permission to the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> AddPermissionToRoleAsync(long roleId, long permissionId);

    /// <summary>Removes the assignment of the permission from the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> RemovePermissionFromRoleAsync(long roleId, long permissionId);
}

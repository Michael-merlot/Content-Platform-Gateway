using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to user permissions.</summary>
public interface IPermissionRepository
{
    /// <summary>Gets the permission.</summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>The permission of an error.</returns>
    Task<Result<Permission, AuthorizationManagementError>> GetPermissionAsync(long permissionId);

    /// <summary>Gets all permissions.</summary>
    /// <returns>The collection of all permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetPermissionsAsync();

    /// <summary>Creates the permission.</summary>
    /// <param name="name">The name of the permission.</param>
    /// <param name="description">The description of the permission.</param>
    /// <returns>The created permission or an error.</returns>
    Task<Result<Permission, AuthorizationManagementError>> CreatePermissionAsync(string name, string? description);

    /// <summary>Deletes the permission.</summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> DeletePermissionAsync(long permissionId);
}

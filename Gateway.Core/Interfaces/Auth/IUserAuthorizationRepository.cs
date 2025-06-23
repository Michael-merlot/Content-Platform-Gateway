using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to user authorization data.</summary>
public interface IUserAuthorizationRepository
{
    /// <summary>Gets the roles assigned to the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of roles associated with the user or an error.</returns>
    /// <remarks>
    /// The method can't verify that the user exists without a request to the identity server and it doesn't call it.
    /// </remarks>
    Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetUserRolesAsync(int userId);

    /// <summary>Gets the permissions assigned to the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of permissions associated with the user or an error.</returns>
    /// <remarks>
    /// The method can't verify that the user exists without a request to the identity server and it doesn't call it.
    /// </remarks>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetUserPermissionsAsync(int userId);

    /// <summary>Assigns the role to the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of a role.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    /// <remarks>
    /// The method can't verify that the user exists without a request to the identity server and it doesn't call it.
    /// </remarks>
    Task<Result<AuthorizationManagementError>> AddRoleToUserAsync(int userId, long roleId);

    /// <summary>Removes the assignment of the role from the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of a role.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    /// <remarks>
    /// The method can't verify that the user exists without a request to the identity server and it doesn't call it.
    /// </remarks>
    Task<Result<AuthorizationManagementError>> RemoveRoleFromUserAsync(int userId, long roleId);
}

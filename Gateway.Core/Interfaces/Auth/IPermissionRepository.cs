using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to user permissions.</summary>
public interface IPermissionRepository
{
    /// <summary>Gets the permissions assigned to the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of permissions associated with the user.</returns>
    Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId);
}

using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to user roles.</summary>
public interface IRoleRepository
{
    /// <summary>Gets the roles assigned to the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of roles associated with the user.</returns>
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
}

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <inheritdoc/>
public class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _authDbContext;

    public PermissionRepository(AuthDbContext authDbContext) =>
        _authDbContext = authDbContext;

    /// <inheritdoc/>
    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId) =>
        await _authDbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission)
            .Distinct()
            .ToListAsync();
}

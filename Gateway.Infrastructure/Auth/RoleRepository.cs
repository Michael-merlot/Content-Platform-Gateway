using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <inheritdoc/>
public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _authDbContext;

    public RoleRepository(AuthDbContext authDbContext) =>
        _authDbContext = authDbContext;

    /// <inheritdoc/>
    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId) =>
        await _authDbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync();
}

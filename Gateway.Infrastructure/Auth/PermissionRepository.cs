using EntityFramework.Exceptions.Common;

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
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
    public async Task<Result<Permission, AuthorizationManagementError>> GetPermissionAsync(long permissionId)
    {
        Permission? permission = await _authDbContext.Permissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == permissionId);

        if (permission is null)
            return AuthorizationManagementError.PermissionNotFound;

        return permission;
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetPermissionsAsync() =>
        await _authDbContext.Permissions.AsNoTracking().ToListAsync();

    /// <inheritdoc/>
    public async Task<Result<Permission, AuthorizationManagementError>> CreatePermissionAsync(string name, string? description)
    {
        Permission permission = new()
        {
            Name = name,
            Description = description
        };

        _authDbContext.Permissions.Add(permission);

        try
        {
            await _authDbContext.SaveChangesAsync();

            return permission;
        }
        catch (UniqueConstraintException)
        {
            return AuthorizationManagementError.EntityAlreadyExists;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> DeletePermissionAsync(long permissionId)
    {
        int deletedCount = await _authDbContext.Permissions.AsNoTracking().Where(x => x.Id == permissionId).ExecuteDeleteAsync();

        return deletedCount < 1
            ? AuthorizationManagementError.PermissionNotFound
            : Result<AuthorizationManagementError>.Success();
    }
}

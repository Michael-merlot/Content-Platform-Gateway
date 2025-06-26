using EntityFramework.Exceptions.Common;

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;
using Gateway.Infrastructure.Persistence.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <inheritdoc/>
public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _authDbContext;

    public RoleRepository(AuthDbContext authDbContext) =>
        _authDbContext = authDbContext;

    /// <inheritdoc/>
    public async Task<Result<Role, AuthorizationManagementError>> GetRoleAsync(long roleId)
    {
        Role? role = await _authDbContext.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == roleId);

        if (role is null)
            return AuthorizationManagementError.RoleNotFound;

        return role;
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetRolesAsync() =>
        await _authDbContext.Roles.AsNoTracking().ToListAsync();

    /// <inheritdoc/>
    public async Task<Result<Role, AuthorizationManagementError>> CreateRoleAsync(string name)
    {
        Role role = new()
        {
            Name = name
        };

        _authDbContext.Roles.Add(role);

        try
        {
            await _authDbContext.SaveChangesAsync();

            return role;
        }
        catch (UniqueConstraintException)
        {
            return AuthorizationManagementError.EntityAlreadyExists;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> DeleteRoleAsync(long roleId)
    {
        int deletedCount = await _authDbContext.Roles
            .AsNoTracking()
            .Where(x => x.Id == roleId)
            .ExecuteDeleteAsync();

        return deletedCount < 1
            ? AuthorizationManagementError.RoleNotFound
            : Result<AuthorizationManagementError>.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetRolePermissionsAsync(long roleId)
    {
        if (!await _authDbContext.Roles.AsNoTracking()
            .AnyAsync(x => x.Id == roleId))
            return AuthorizationManagementError.RoleNotFound;

        return await _authDbContext.Roles
            .AsNoTracking()
            .Where(x => x.Id == roleId)
            .SelectMany(x => x.RolePermissions.Select(x => x.Permission))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> AddPermissionToRoleAsync(long roleId, long permissionId)
    {
        RolePermission rolePermission = new()
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _authDbContext.RolePermissions.Add(rolePermission);

        try
        {
            await _authDbContext.SaveChangesAsync();

            return Result<AuthorizationManagementError>.Success();
        }
        catch (ReferenceConstraintException)
        {
            return AuthorizationManagementError.AnyEntityNotFound;
        }
        catch (UniqueConstraintException)
        {
            return AuthorizationManagementError.EntityAlreadyExists;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> RemovePermissionFromRoleAsync(long roleId, long permissionId)
    {
        int deletedCount = await _authDbContext.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId && x.PermissionId == permissionId)
            .ExecuteDeleteAsync();

        return deletedCount < 1
            ? AuthorizationManagementError.AnyEntityNotFound
            : Result<AuthorizationManagementError>.Success();
    }
}

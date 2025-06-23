using EntityFramework.Exceptions.Common;

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <inheritdoc/>
public class UserAuthorizationRepository : IUserAuthorizationRepository
{
    private readonly AuthDbContext _authDbContext;

    public UserAuthorizationRepository(AuthDbContext authDbContext) =>
        _authDbContext = authDbContext;

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetUserRolesAsync(int userId) =>
        // Can't verify that the user exists without a request to the identity server
        await _authDbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetUserPermissionsAsync(int userId) =>
        // Can't verify that the user exists without a request to the identity server
        await _authDbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission)
            .Distinct()
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> AddRoleToUserAsync(int userId, long roleId)
    {
        // Can't verify that the user exists without a request to the identity server

        UserRole userRole = new()
        {
            UserId = userId,
            RoleId = roleId
        };

        _authDbContext.UserRoles.Add(userRole);

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
    public async Task<Result<AuthorizationManagementError>> RemoveRoleFromUserAsync(int userId, long roleId)
    {
        // Can't verify that the user exists without a request to the identity server

        int deletedCount = await _authDbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.RoleId == roleId)
            .ExecuteDeleteAsync();

        return deletedCount < 1
            ? AuthorizationManagementError.AnyEntityNotFound
            : Result<AuthorizationManagementError>.Success();
    }
}

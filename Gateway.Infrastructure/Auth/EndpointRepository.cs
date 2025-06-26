using EntityFramework.Exceptions.Common;

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;
using Gateway.Infrastructure.Persistence.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <inheritdoc/>
public class EndpointRepository : IEndpointRepository
{
    private readonly AuthDbContext _authDbContext;

    public EndpointRepository(AuthDbContext authDbContext) =>
        _authDbContext = authDbContext;

    /// <inheritdoc/>
    public async Task<Result<Endpoint, AuthorizationManagementError>> GetEndpointAsync(long endpointId)
    {
        Endpoint? endpoint = await _authDbContext.Endpoints.AsNoTracking().FirstOrDefaultAsync(x => x.Id == endpointId);

        if (endpoint is null)
            return AuthorizationManagementError.EndpointNotFound;

        return endpoint;
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Endpoint>, AuthorizationManagementError>> GetEndpointsAsync() =>
        await _authDbContext.Endpoints.AsNoTracking().ToListAsync();

    /// <inheritdoc/>
    public async Task<Result<Endpoint, AuthorizationManagementError>> CreateEndpointAsync(string controller, string action,
        string httpMethod)
    {
        Endpoint endpoint = new()
        {
            Controller = controller,
            Action = action,
            HttpMethod = httpMethod
        };

        _authDbContext.Endpoints.Add(endpoint);

        try
        {
            await _authDbContext.SaveChangesAsync();

            return endpoint;
        }
        catch (UniqueConstraintException)
        {
            return AuthorizationManagementError.EntityAlreadyExists;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> DeleteEndpointAsync(long endpointId)
    {
        int deletedCount = await _authDbContext.Endpoints.AsNoTracking().Where(x => x.Id == endpointId).ExecuteDeleteAsync();

        return deletedCount < 1
            ? AuthorizationManagementError.EndpointNotFound
            : Result<AuthorizationManagementError>.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(long endpointId)
    {
        if (!await _authDbContext.Endpoints.AsNoTracking().AnyAsync(x => x.Id == endpointId))
            return AuthorizationManagementError.EndpointNotFound;

        return await _authDbContext.Endpoints
            .AsNoTracking()
            .Where(x => x.Id == endpointId)
            .SelectMany(x => x.EndpointPermissions.Select(x => x.Permission))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(
        string controller, string action,
        string httpMethod)
    {
        if (!await _authDbContext.Endpoints.AsNoTracking()
            .AnyAsync(x => x.Controller == controller && x.Action == action && x.HttpMethod == httpMethod))
            return AuthorizationManagementError.EndpointNotFound;

        return await _authDbContext.Endpoints
            .AsNoTracking()
            .Where(x => x.Controller == controller && x.Action == action && x.HttpMethod == httpMethod)
            .SelectMany(x => x.EndpointPermissions.Select(x => x.Permission))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> AddPermissionRequirementToEndpointAsync(long endpointId, long permissionId)
    {
        EndpointPermission endpointPermission = new()
        {
            EndpointId = endpointId,
            PermissionId = permissionId
        };

        _authDbContext.EndpointPermissions.Add(endpointPermission);

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
    public async Task<Result<AuthorizationManagementError>> RemovePermissionRequirementFromEndpointAsync(long endpointId, long permissionId)
    {
        int deletedCount = await _authDbContext.EndpointPermissions
            .AsNoTracking()
            .Where(x => x.EndpointId == endpointId && x.PermissionId == permissionId)
            .ExecuteDeleteAsync();

        return deletedCount < 1
            ? AuthorizationManagementError.AnyEntityNotFound
            : Result<AuthorizationManagementError>.Success();
    }
}

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Interfaces.Persistence;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Services.Auth;

/// <inheritdoc/>
public class AuthorizationManagementService : IAuthorizationManagementService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IEndpointRepository _endpointRepository;
    private readonly IUserAuthorizationRepository _userAuthorizationRepository;
    private readonly IDistributedCacheService _cache;

    private static TimeSpan CacheExpiration => TimeSpan.FromMinutes(1);
    private static TimeSpan NegativeCacheExpiration => TimeSpan.FromSeconds(20);

    public AuthorizationManagementService(IRoleRepository roleRepository, IPermissionRepository permissionRepository,
        IEndpointRepository endpointRepository, IUserAuthorizationRepository userAuthorizationRepository, IDistributedCacheService cache)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _endpointRepository = endpointRepository;
        _userAuthorizationRepository = userAuthorizationRepository;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<Result<Role, AuthorizationManagementError>> GetRoleAsync(long roleId) =>
        await _roleRepository.GetRoleAsync(roleId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetRolesAsync() =>
        await _roleRepository.GetRolesAsync();

    /// <inheritdoc/>
    public async Task<Result<Role, AuthorizationManagementError>> CreateRoleAsync(string name)
    {
        if (name.Length > 100)
            return AuthorizationManagementError.BadRequest;

        return await _roleRepository.CreateRoleAsync(name);
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> DeleteRoleAsync(long roleId) =>
        await _roleRepository.DeleteRoleAsync(roleId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetRolePermissionsAsync(long roleId) =>
        await _roleRepository.GetRolePermissionsAsync(roleId);

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> AddPermissionToRoleAsync(long roleId, long permissionId) =>
        await _roleRepository.AddPermissionToRoleAsync(roleId, permissionId);

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> RemovePermissionFromRoleAsync(long roleId, long permissionId) =>
        await _roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Role>, AuthorizationManagementError>> GetUserRolesAsync(int userId, bool useCache = true)
    {
        if (!useCache)
            return await _userAuthorizationRepository.GetUserRolesAsync(userId);

        string cacheKey = $"auth:user:{userId}:roles";

        if (await _cache.GetAsync<bool?>($"{cacheKey}:error") == true)
            return AuthorizationManagementError.UserNotFound;

        IEnumerable<Role>? roles = await _cache.GetAsync<IEnumerable<Role>>(cacheKey);

        if (roles is not null)
            return Result<IEnumerable<Role>, AuthorizationManagementError>.Success(roles);

        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _userAuthorizationRepository.GetUserRolesAsync(userId);

        if (!result.IsSuccess)
        {
            if (result.Error == AuthorizationManagementError.UserNotFound)
                await _cache.SetAsync($"{cacheKey}:error", true, NegativeCacheExpiration);

            return result;
        }

        await _cache.SetAsync(cacheKey, result.Value!, CacheExpiration);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetUserPermissionsAsync(int userId,
        bool useCache = true)
    {
        if (!useCache)
            return await _userAuthorizationRepository.GetUserPermissionsAsync(userId);

        string cacheKey = $"auth:user:{userId}:permissions";

        if (await _cache.GetAsync<bool?>($"{cacheKey}:error") == true)
            return AuthorizationManagementError.UserNotFound;

        IEnumerable<Permission>? permissions = await _cache.GetAsync<IEnumerable<Permission>>(cacheKey);

        if (permissions is not null)
            return Result<IEnumerable<Permission>, AuthorizationManagementError>.Success(permissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _userAuthorizationRepository.GetUserPermissionsAsync(userId);

        if (!result.IsSuccess)
        {
            if (result.Error == AuthorizationManagementError.UserNotFound)
                await _cache.SetAsync($"{cacheKey}:error", true, NegativeCacheExpiration);

            return result;
        }

        await _cache.SetAsync(cacheKey, result.Value!, CacheExpiration);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> AddRoleToUserAsync(int userId, long roleId) =>
        await _userAuthorizationRepository.AddRoleToUserAsync(userId, roleId);

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> RemoveRoleFromUserAsync(int userId, long roleId) =>
        await _userAuthorizationRepository.RemoveRoleFromUserAsync(userId, roleId);

    /// <inheritdoc/>
    public async Task<Result<Permission, AuthorizationManagementError>> GetPermissionAsync(long permissionId) =>
        await _permissionRepository.GetPermissionAsync(permissionId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetPermissionsAsync() =>
        await _permissionRepository.GetPermissionsAsync();

    /// <inheritdoc/>
    public async Task<Result<Permission, AuthorizationManagementError>> CreatePermissionAsync(string name, string? description)
    {
        if (name.Length > 100 || description?.Length > 500)
            return AuthorizationManagementError.BadRequest;

        return await _permissionRepository.CreatePermissionAsync(name, description);
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> DeletePermissionAsync(long permissionId) =>
        await _permissionRepository.DeletePermissionAsync(permissionId);

    /// <inheritdoc/>
    public async Task<Result<Endpoint, AuthorizationManagementError>> GetEndpointAsync(long endpointId) =>
        await _endpointRepository.GetEndpointAsync(endpointId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Endpoint>, AuthorizationManagementError>> GetEndpointsAsync() =>
        await _endpointRepository.GetEndpointsAsync();

    /// <inheritdoc/>
    public async Task<Result<Endpoint, AuthorizationManagementError>> CreateEndpointAsync(string controller, string action,
        string httpMethod)
    {
        if (controller.Length > 100 || action.Length > 100 || httpMethod.Length > 100)
            return AuthorizationManagementError.BadRequest;

        return await _endpointRepository.CreateEndpointAsync(controller, action, httpMethod);
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> DeleteEndpointAsync(long endpointId) =>
        await _endpointRepository.DeleteEndpointAsync(endpointId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>>
        GetEndpointPermissionRequirementsAsync(long endpointId) =>
        await _endpointRepository.GetEndpointPermissionRequirementsAsync(endpointId);

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(
        string controller, string action, string httpMethod, bool useCache = true)
    {
        if (!useCache)
            return await _endpointRepository.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod);

        string cacheKey = $"auth:endpoints:{controller}/{action}/{httpMethod}:permissions";

        if (await _cache.GetAsync<bool?>($"{cacheKey}:error") == true)
            return AuthorizationManagementError.EndpointNotFound;

        IEnumerable<Permission>? permissions = await _cache.GetAsync<IEnumerable<Permission>>(cacheKey);

        if (permissions is not null)
            return Result<IEnumerable<Permission>, AuthorizationManagementError>.Success(permissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _endpointRepository.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod);

        if (!result.IsSuccess)
        {
            if (result.Error == AuthorizationManagementError.EndpointNotFound)
                await _cache.SetAsync($"{cacheKey}:error", true, NegativeCacheExpiration);

            return result;
        }

        await _cache.SetAsync(cacheKey, result.Value!, CacheExpiration);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>> AddPermissionRequirementToEndpointAsync(long endpointId, long permissionId) =>
        await _endpointRepository.AddPermissionRequirementToEndpointAsync(endpointId, permissionId);

    /// <inheritdoc/>
    public async Task<Result<AuthorizationManagementError>>
        RemovePermissionRequirementFromEndpointAsync(long endpointId, long permissionId) =>
        await _endpointRepository.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId);
}

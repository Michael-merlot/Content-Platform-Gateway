using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Security.Claims;

namespace Gateway.Api.Auth;

/// <summary>Events for the <c>JwtBearer</c> that enrich the claims with user's roles and permissions.</summary>
public class PermissionEnrichmentJwtBearerEvents : JwtBearerEvents
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public PermissionEnrichmentJwtBearerEvents(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    /// <inheritdoc/>
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        string? userIdString = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdString is null)
            return;

        if (!Int32.TryParse(userIdString, out int userId))
            return;

        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return;

        IEnumerable<Role> roles = await _roleRepository.GetRolesByUserIdAsync(userId);

        foreach (Role role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

            if (role.IsAdmin && !identity.HasClaim(x => x.Type == ExtraClaimTypes.IsAdmin))
                identity.AddClaim(new Claim(ExtraClaimTypes.IsAdmin, true.ToString()));
        }

        IEnumerable<Permission> permissions = await _permissionRepository.GetPermissionsByUserIdAsync(userId);

        foreach (Permission permission in permissions)
            identity.AddClaim(new Claim(ExtraClaimTypes.Permission, permission.Name));
    }
}

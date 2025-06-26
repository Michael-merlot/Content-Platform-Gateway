using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Security.Claims;

namespace Gateway.Api.Auth;

/// <summary>Events for the <c>JwtBearer</c> that enrich the claims with user's roles and permissions.</summary>
public class PermissionEnrichmentJwtBearerEvents : JwtBearerEvents
{
    private readonly IAuthorizationManagementService _authorizationManagementService;

    public PermissionEnrichmentJwtBearerEvents(IAuthorizationManagementService authorizationManagementService) =>
        _authorizationManagementService = authorizationManagementService;

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

        Result<IEnumerable<Role>, AuthorizationManagementError> rolesResult =
            await _authorizationManagementService.GetUserRolesAsync(userId);

        if (rolesResult.IsSuccess)
        {
            foreach (Role role in rolesResult.Value!)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

                if (role.IsAdmin && !identity.HasClaim(x => x.Type == ExtraClaimTypes.IsAdmin))
                    identity.AddClaim(new Claim(ExtraClaimTypes.IsAdmin, true.ToString()));
            }
        }

        Result<IEnumerable<Permission>, AuthorizationManagementError> permissionsResult =
            await _authorizationManagementService.GetUserPermissionsAsync(userId);

        if (!permissionsResult.IsSuccess)
            return;

        foreach (Permission permission in permissionsResult.Value!)
            identity.AddClaim(new Claim(ExtraClaimTypes.Permission, permission.Name));
    }
}

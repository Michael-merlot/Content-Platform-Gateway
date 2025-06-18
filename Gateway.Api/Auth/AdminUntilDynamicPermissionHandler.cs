using Gateway.Core.Interfaces.Auth;

using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>Authorization handler for <see cref="AdminUntilDynamicPermissionRequirement"/>.</summary>
public class AdminUntilDynamicPermissionHandler : BaseDynamicPermissionHandler<AdminUntilDynamicPermissionRequirement>
{
    public AdminUntilDynamicPermissionHandler(IEndpointRepository endpointRepository) : base(endpointRepository) { }

    /// <inheritdoc/>
    protected override bool HandleEmptyEndpointRequirements(AuthorizationHandlerContext context,
        AdminUntilDynamicPermissionRequirement requirement) =>
        false;
}

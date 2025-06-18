using Gateway.Core.Interfaces.Auth;

using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>Authorization handler for <see cref="DynamicPermissionRequirement"/>.</summary>
public class DynamicPermissionHandler : BaseDynamicPermissionHandler<DynamicPermissionRequirement>
{
    public DynamicPermissionHandler(IEndpointRepository endpointRepository) : base(endpointRepository) { }

    /// <inheritdoc/>
    protected override bool HandleEmptyEndpointRequirements(AuthorizationHandlerContext context,
        DynamicPermissionRequirement requirement) =>
        true;
}

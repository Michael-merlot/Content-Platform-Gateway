using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gateway.Api.Auth;

/// <summary>Authorization handler for <see cref="DefaultDynamicPermissionRequirement"/>.</summary>
public class DefaultDynamicPermissionHandler : BaseDynamicPermissionHandler<DefaultDynamicPermissionRequirement>
{
    public DefaultDynamicPermissionHandler(IAuthorizationManagementService authorizationManagementService) : base(
        authorizationManagementService) { }

    /// <inheritdoc/>
    protected override async Task<Result<List<string>, RequiredPermissionsBuildError>> BuildRequiredPermissions(
        AuthorizationHandlerContext context, DefaultDynamicPermissionRequirement requirement)
    {
        Result<List<string>, RequiredPermissionsBuildError> requiredPermissionsBuildResult =
            await base.BuildRequiredPermissions(context, requirement);

        if (requiredPermissionsBuildResult.IsSuccess ||
            requiredPermissionsBuildResult.Error != RequiredPermissionsBuildError.EndpointNotFound)
            return requiredPermissionsBuildResult;

        DefaultDynamicPermissionAuthorizeAttribute? defaultPermissionAttribute = context.Resource switch
        {
            HttpContext httpContext => httpContext.GetEndpoint()?.Metadata.GetMetadata<DefaultDynamicPermissionAuthorizeAttribute>(),
            AuthorizationFilterContext afc => afc.HttpContext.GetEndpoint()
                ?.Metadata.GetMetadata<DefaultDynamicPermissionAuthorizeAttribute>(),
            _ => null
        };

        return defaultPermissionAttribute?.Permissions.ToList() ?? requiredPermissionsBuildResult;
    }
}

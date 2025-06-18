using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Gateway.Api.Auth;

/// <summary>Authorization handler for <see cref="DynamicPermissionRequirement"/>.</summary>
public class DynamicPermissionHandler : AuthorizationHandler<DynamicPermissionRequirement>
{
    private readonly IEndpointRepository _endpointRepository;

    public DynamicPermissionHandler(IEndpointRepository endpointRepository) =>
        _endpointRepository = endpointRepository;

    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DynamicPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        if (context.Resource is not HttpContext httpContext)
            return;

        ControllerActionDescriptor? descriptor = httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        string? controller = descriptor?.ControllerName;
        string? action = descriptor?.ActionName;
        string httpMethod = httpContext.Request.Method;

        if (controller == null || action == null)
            return;

        if (context.User.HasClaim(ExtraClaimTypes.IsAdmin, true.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        List<string> requiredPermissionsNames = (await _endpointRepository.GetRequiredPermissionsAsync(controller, action, httpMethod))
            .Select(x => x.Name)
            .ToList();

        if (requiredPermissionsNames.Count == 0)
        {
            context.Succeed(requirement);
            return;
        }

        List<string> userPermissionsNames = context.User
            .FindAll(ExtraClaimTypes.Permission)
            .Select(x => x.Value)
            .ToList();

        if (requiredPermissionsNames.All(r => userPermissionsNames.Contains(r)))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}

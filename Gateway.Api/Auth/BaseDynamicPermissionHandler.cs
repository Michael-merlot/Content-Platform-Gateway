using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Gateway.Api.Auth;

/// <summary>Base authorization handler that checks for the endpoint and user permissions dynamically.</summary>
/// <typeparam name="T">The type of the requirement to handle.</typeparam>
public abstract class BaseDynamicPermissionHandler<T> : AuthorizationHandler<T>
    where T : IAuthorizationRequirement
{
    protected readonly IEndpointRepository EndpointRepository;

    protected BaseDynamicPermissionHandler(IEndpointRepository endpointRepository) =>
        EndpointRepository = endpointRepository;

    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        T requirement)
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

        List<string> requiredPermissionsNames = (await EndpointRepository.GetRequiredPermissionsAsync(controller, action, httpMethod))
            .Select(x => x.Name)
            .ToList();

        if (requiredPermissionsNames.Count == 0)
        {
            bool isSuccess = HandleEmptyEndpointRequirements(context, requirement);

            if (isSuccess)
                context.Succeed(requirement);
            else
                context.Fail();

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

    /// <summary>
    /// Makes a decision if authorization is allowed when the endpoint requirements are empty. Should not call
    /// <see cref="AuthorizationHandlerContext.Succeed(IAuthorizationRequirement)"/> or <see cref="AuthorizationHandlerContext.Fail()"/>.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns><c>true</c> if authorization is allowed, <c>false</c> otherwise.</returns>
    protected virtual bool HandleEmptyEndpointRequirements(AuthorizationHandlerContext context, T requirement) =>
        true;
}

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gateway.Api.Auth;

/// <summary>Base authorization handler that checks for the endpoint and user permissions dynamically.</summary>
/// <typeparam name="T">The type of the requirement to handle.</typeparam>
public abstract class BaseDynamicPermissionHandler<T> : AuthorizationHandler<T>
    where T : IAuthorizationRequirement
{
    protected readonly IAuthorizationManagementService AuthorizationManagementService;

    protected BaseDynamicPermissionHandler(IAuthorizationManagementService authorizationManagementService) =>
        AuthorizationManagementService = authorizationManagementService;

    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        T requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return;
        }

        if (context.User.HasClaim(ExtraClaimTypes.IsAdmin, true.ToString()))
        {
            context.Succeed(requirement);
            return;
        }

        Result<List<string>, RequiredPermissionsBuildError> requiredPermissionsResult =
            await BuildRequiredPermissions(context, requirement);

        if (!requiredPermissionsResult.IsSuccess)
            return;

        List<string> requiredPermissionsNames = requiredPermissionsResult.Value!;

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

    /// <summary>Builds a collection of required permissions.</summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns>
    /// The collection of permission requirements or <c>null</c> if it's impossible to build it for the given context.
    /// </returns>
    protected virtual async Task<Result<List<string>, RequiredPermissionsBuildError>> BuildRequiredPermissions(
        AuthorizationHandlerContext context, T requirement)
    {
        string? controller;
        string? action;
        string httpMethod;

        switch (context.Resource)
        {
            case HttpContext httpContext:
            {
                ControllerActionDescriptor? controllerActionDescriptor =
                    httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();

                controller = controllerActionDescriptor?.ControllerName;
                action = controllerActionDescriptor?.ActionName;
                httpMethod = httpContext.Request.Method;

                break;
            }
            case AuthorizationFilterContext { ActionDescriptor: ControllerActionDescriptor controllerActionDescriptor } afc:
            {
                controller = controllerActionDescriptor.ControllerName;
                action = controllerActionDescriptor.ActionName;
                httpMethod = afc.HttpContext.Request.Method;

                break;
            }
            default:
                return RequiredPermissionsBuildError.InappropriateContext;
        }

        if (controller is null || action is null)
            return RequiredPermissionsBuildError.InappropriateContext;

        Result<IEnumerable<Permission>, AuthorizationManagementError> endpointPermissionRequirementsResult =
            await AuthorizationManagementService.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod);

        return endpointPermissionRequirementsResult.Match<Result<List<string>, RequiredPermissionsBuildError>>(
            value => value.Select(x => x.Name).ToList(),
            _ => RequiredPermissionsBuildError.EndpointNotFound);
    }
}

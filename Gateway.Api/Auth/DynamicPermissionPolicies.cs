using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>Defines constants for the known dynamic permission policies that can be used to authorize the user.</summary>
public static class DynamicPermissionPolicies
{
    /// <summary>The name of the policy which requires dynamic permission.</summary>
    public const string RequireDynamicPermissionPolicyName = "RequireDynamicPermission";

    /// <summary>The policy which requires dynamic permission.</summary>
    public static AuthorizationPolicy RequireDynamicPermissionPolicy =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new DynamicPermissionRequirement())
            .Build();

    /// <summary>The name of the policy which requires an admin user.</summary>
    public const string RequireAdminPolicyName = "RequireAdmin";

    /// <summary>The policy which requires an admin user.</summary>
    public static AuthorizationPolicy RequireAdminPolicy =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(ExtraClaimTypes.IsAdmin, true.ToString())
            .Build();

    /// <summary>The name of the policy which requires an admin user until the endpoint is configured dynamically.</summary>
    public const string RequireAdminUntilDynamicPolicyName = "RequireAdminUntilDynamic";

    /// <summary>The policy which requires an admin user until dynamic permission is configured.</summary>
    public static AuthorizationPolicy RequireAdminUntilDynamicPolicy =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new AdminUntilDynamicPermissionRequirement())
            .Build();
}

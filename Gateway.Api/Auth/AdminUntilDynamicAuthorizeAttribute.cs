using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>
/// Specifies that the class or method that this attribute is applied to requires an admin user until the endpoint permissions are configured.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AdminUntilDynamicAuthorizeAttribute : AuthorizeAttribute
{
    public AdminUntilDynamicAuthorizeAttribute() =>
        Policy = DynamicPermissionPolicies.RequireAdminUntilDynamicPolicyName;
}

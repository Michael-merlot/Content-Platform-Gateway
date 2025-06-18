using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>Specifies that the class or method that this attribute is applied to requires an admin user.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AdminAuthorizeAttribute : AuthorizeAttribute
{
    public AdminAuthorizeAttribute() =>
        Policy = DynamicPermissionPolicies.RequireAdminPolicyName;
}

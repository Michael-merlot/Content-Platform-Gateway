using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>
/// Specifies that the class or method that this attribute is applied to will use default permission requirements if dynamic are not
/// configured.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class DefaultDynamicPermissionAuthorizeAttribute : AuthorizeAttribute
{
    public string[] Permissions { get; set; }

    public DefaultDynamicPermissionAuthorizeAttribute(params string[] permissions)
    {
        Permissions = permissions;
        Policy = DynamicPermissionPolicies.RequireDynamicPermissionWithDefaultPolicyName;
    }
}

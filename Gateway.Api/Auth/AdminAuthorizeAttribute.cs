using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>Specifies that the class or method that this attribute is applied to requires an admin user.</summary>
public class AdminAuthorizeAttribute : AuthorizeAttribute
{
    public AdminAuthorizeAttribute() =>
        Policy = PolicyNames.RequireAdmin;
}

using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>
/// Represents the requirement that permissions to execute the endpoint will be checked dynamically and if they're not found, an admin user is
/// required.
/// </summary>
public class AdminUntilDynamicPermissionRequirement : IAuthorizationRequirement;

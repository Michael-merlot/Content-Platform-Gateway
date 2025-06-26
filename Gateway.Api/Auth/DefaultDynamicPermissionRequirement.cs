using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>
/// Represents the requirement that permissions to execute the endpoint will be checked dynamically and the defaults defined per endpoint will
/// be used if dynamic permissions are not configured
/// </summary>
public class DefaultDynamicPermissionRequirement : IAuthorizationRequirement;

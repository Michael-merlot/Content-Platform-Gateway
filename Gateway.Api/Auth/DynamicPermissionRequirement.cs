using Microsoft.AspNetCore.Authorization;

namespace Gateway.Api.Auth;

/// <summary>Represents the requirement that permissions to execute the endpoint will be checked dynamically.</summary>
public class DynamicPermissionRequirement : IAuthorizationRequirement;

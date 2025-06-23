namespace Gateway.Api.Models.Auth;

/// <summary>Role collection response for an admin.</summary>
/// <param name="Roles">The collection of roles.</param>
public sealed record RoleCollectionAdminResponse(IEnumerable<RoleAdminDto> Roles);

namespace Gateway.Api.Models.Auth;

/// <summary>The permission collection response.</summary>
/// <param name="Permissions">The collection of permissions.</param>
public sealed record PermissionCollectionResponse(IEnumerable<PermissionDto> Permissions);

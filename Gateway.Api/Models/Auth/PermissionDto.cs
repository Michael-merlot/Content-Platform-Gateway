namespace Gateway.Api.Models.Auth;

/// <summary>Represents the permission.</summary>
/// <param name="Id">The unique identifier of the permission.</param>
/// <param name="Name">The name of the permission.</param>
public sealed record PermissionDto(
    long Id,
    string Name
);

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a role for an admin.</summary>
/// <param name="Id">The unique identifier of the role.</param>
/// <param name="Name">The name of the role.</param>
/// <param name="IsAdmin">Specifies, whether the role is an admin role.</param>
public sealed record RoleAdminDto(
    long Id,
    string Name,
    bool IsAdmin
);

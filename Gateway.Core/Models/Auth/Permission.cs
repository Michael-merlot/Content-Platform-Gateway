namespace Gateway.Core.Models.Auth;

/// <summary>Represents an authorization permission.</summary>
public class Permission
{
    /// <summary>The unique identifier of this permission.</summary>
    public long Id { get; set; }

    /// <summary>The unique name of this permission.</summary>
    public required string Name { get; set; }

    /// <summary>A brief description of this permission.</summary>
    public string? Description { get; set; }

    /// <summary>The associations between this permission and roles.</summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = null!;

    /// <summary>The associations between this permission and endpoints.</summary>
    public ICollection<EndpointPermission> EndpointPermissions { get; set; } = null!;
}

namespace Gateway.Core.Models.Auth;

/// <summary>Represents the association between a role and a permission in the system.</summary>
public class RolePermission
{
    /// <summary>Unique identifier of the role in this association.</summary>
    public long RoleId { get; set; }

    /// <summary>Unique identifier of the permission in this association.</summary>
    public long PermissionId { get; set; }

    /// <summary>Navigation property to the associated role.</summary>
    public Role Role { get; set; } = null!;

    /// <summary>Navigation property to the associated permission.</summary>
    public Permission Permission { get; set; } = null!;
}

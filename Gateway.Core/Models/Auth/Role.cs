namespace Gateway.Core.Models.Auth;

/// <summary>Represents a user role.</summary>
public class Role
{
    /// <summary>Unique identifier for the role.</summary>
    public long Id { get; set; }

    /// <summary>The unique name of this role.</summary>
    public required string Name { get; set; }

    /// <summary>Specifies whether or not this role is an admin role.</summary>
    public bool IsAdmin { get; set; }

    /// <summary>The assotiations between users and this role.</summary>
    public ICollection<UserRole> UserRoles { get; set; } = null!;

    /// <summary>The associations between this role and its permissions.</summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = null!;
}

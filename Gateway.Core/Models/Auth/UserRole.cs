namespace Gateway.Core.Models.Auth;

/// <summary>Represents the association between a user and a role.</summary>
public class UserRole
{
    /// <summary>Unique identifier of the user in this association.</summary>
    public int UserId { get; set; }

    /// <summary>Unique identifier of the role in this association.</summary>
    public long RoleId { get; set; }

    /// <summary>Navigation property for the associated role.</summary>
    public Role Role { get; set; } = null!;
}

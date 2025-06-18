namespace Gateway.Core.Models.Auth;

/// <summary>Represents the association between an API endpoint and a permission.</summary>
public class EndpointPermission
{
    /// <summary>Unique identifier of the endpoint in this association.</summary>
    public long EndpointId { get; set; }

    /// <summary>Unique identifier of the permission in this association.</summary>
    public long PermissionId { get; set; }

    /// <summary>Navigation property to the associated endpoint.</summary>
    public Endpoint Endpoint { get; set; } = null!;

    /// <summary>Navigation property to the associated permission.</summary>
    public Permission Permission { get; set; } = null!;
}

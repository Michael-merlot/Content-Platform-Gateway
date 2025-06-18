namespace Gateway.Core.Models.Auth;

/// <summary>Represents an API endpoint.</summary>
public class Endpoint
{
    /// <summary>Unique identifier of this endpoint.</summary>
    public long Id { get; set; }

    /// <summary>Controller name for this endpoint.</summary>
    public required string Controller { get; set; }

    /// <summary>Action name for this endpoint.</summary>
    public required string Action { get; set; }

    /// <summary>HTTP method (e.g., GET, POST) for this endpoint.</summary>
    public required string HttpMethod { get; set; }

    /// <summary>The associations between this endpoint and permissions.</summary>
    public ICollection<EndpointPermission> EndpointPermissions { get; set; } = null!;
}

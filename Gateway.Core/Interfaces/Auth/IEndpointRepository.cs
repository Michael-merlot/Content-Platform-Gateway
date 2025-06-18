using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to endpoints.</summary>
public interface IEndpointRepository
{
    /// <summary>Gets the permissions required to execute a specific endpoint.</summary>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="httpMethod">The HTTP method (e.g., GET, POST).</param>
    /// <returns>A collection of permissions required to execute the endpoint.</returns>
    Task<IEnumerable<Permission>> GetRequiredPermissionsAsync(string controller, string action, string httpMethod);
}

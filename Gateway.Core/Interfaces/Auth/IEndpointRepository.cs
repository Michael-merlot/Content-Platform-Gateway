using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

namespace Gateway.Core.Interfaces.Auth;

/// <summary>Repository for operations related to endpoints.</summary>
public interface IEndpointRepository
{
    /// <summary>Gets the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>The endpoint or an error.</returns>
    Task<Result<Endpoint, AuthorizationManagementError>> GetEndpointAsync(long endpointId);

    /// <summary>Gets all endpoints.</summary>
    /// <returns>The collection of all endpoints or an error.</returns>
    Task<Result<IEnumerable<Endpoint>, AuthorizationManagementError>> GetEndpointsAsync();

    /// <summary>Creates an endpoint.</summary>
    /// <param name="controller">The controller of an endpoint.</param>
    /// <param name="action">The action of an endpoint.</param>
    /// <param name="httpMethod">The HTTP method of an endpoint.</param>
    /// <returns>The created endpoint or an error.</returns>
    Task<Result<Endpoint, AuthorizationManagementError>> CreateEndpointAsync(string controller, string action, string httpMethod);

    /// <summary>Deletes the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> DeleteEndpointAsync(long endpointId);

    /// <summary>Gets endpoint permission requirements.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>The collection of permissions or an error.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(long endpointId);

    /// <summary>Gets the permissions required to execute a specific endpoint.</summary>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="action">The name of the action.</param>
    /// <param name="httpMethod">The HTTP method (e.g., GET, POST).</param>
    /// <returns>A collection of permissions required to execute the endpoint.</returns>
    Task<Result<IEnumerable<Permission>, AuthorizationManagementError>> GetEndpointPermissionRequirementsAsync(string controller,
        string action,
        string httpMethod);

    /// <summary>Adds permission requirement to the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> AddPermissionRequirementToEndpointAsync(long endpointId, long permissionId);

    /// <summary>Removes the permission requirement from the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>Whether or not the operation was successful; otherwise, an error.</returns>
    Task<Result<AuthorizationManagementError>> RemovePermissionRequirementFromEndpointAsync(long endpointId, long permissionId);
}

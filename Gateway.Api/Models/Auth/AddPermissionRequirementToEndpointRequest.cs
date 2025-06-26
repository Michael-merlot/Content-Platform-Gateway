using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a request to add a permission requirement to the endpoint.</summary>
/// <param name="PermissionId">The unique identifier of the permission.</param>
public sealed record AddPermissionRequirementToEndpointRequest([Required] long PermissionId);

using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a request to add permission to the role.</summary>
/// <param name="PermissionId">The unique identifier of the permission.</param>
public sealed record AddPermissionToRoleRequest([Required] long PermissionId);

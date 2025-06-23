using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a request to add role to the user.</summary>
/// <param name="RoleId">The unique identifier of the role.</param>
public sealed record AddRoleToUserRequest([Required] long RoleId);

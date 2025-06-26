using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a request to create a role.</summary>
/// <param name="Name">The name of a role.</param>
public sealed record CreateRoleRequest([Required] [MaxLength(100)] string Name);

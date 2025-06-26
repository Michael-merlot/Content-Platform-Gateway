using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a request to create a permission.</summary>
/// <param name="Name">The name of a permission.</param>
/// <param name="Description">The description of a permission.</param>
public sealed record CreatePermissionRequest(
    [Required] [MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);

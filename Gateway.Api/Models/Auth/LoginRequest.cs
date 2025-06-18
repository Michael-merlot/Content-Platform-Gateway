using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a login request.</summary>
/// <param name="Email">User e-mail.</param>
/// <param name="Password">User password.</param>
public sealed record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] [MaxLength(100)] string Password
);

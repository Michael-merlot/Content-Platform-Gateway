using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Refresh token request</summary>
/// <param name="RefreshToken">Refresh token</param>
public sealed record RefreshRequest([Required] string RefreshToken);

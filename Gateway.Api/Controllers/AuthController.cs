using Gateway.Api.Mappers;
using Gateway.Api.Models.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

using IAuthenticationService = Gateway.Core.Interfaces.Auth.IAuthenticationService;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("/api/v1/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService) =>
        _authenticationService = authenticationService;

    /// <summary>Authenticates the user</summary>
    /// <param name="loginRequest">Login request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens</returns>
    [HttpPost]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        AuthResult<AuthTokenSession> result = await _authenticationService
            .LoginAsync(loginRequest.Email, loginRequest.Password, cancellationToken);

        return result.IsSuccess ? Ok(result.Data!.MapToAuthResponse()) : result.AsProblemDetails(this);
    }

    /// <summary>Refreshes the session</summary>
    /// <param name="refreshRequest">Refresh request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens</returns>
    [HttpPost]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Refresh(RefreshRequest refreshRequest, CancellationToken cancellationToken = default)
    {
        AuthResult<AuthTokenSession> result = await _authenticationService
            .RefreshAsync(refreshRequest.RefreshToken, cancellationToken);

        return result.IsSuccess ? Ok(result.Data!.MapToAuthResponse()) : result.AsProblemDetails(this);
    }

    /// <summary>Revokes the access token</summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>The access token is fetched from the <c>Authorization</c> header</remarks>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");

        if (String.IsNullOrEmpty(accessToken))
            return Unauthorized();

        AuthResult result = await _authenticationService
            .LogoutAsync(accessToken, cancellationToken);

        return result.IsSuccess ? NoContent() : result.AsProblemDetails(this);
    }
}

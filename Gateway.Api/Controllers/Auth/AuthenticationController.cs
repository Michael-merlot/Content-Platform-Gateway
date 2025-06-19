using Gateway.Api.Mappers;
using Gateway.Api.Models.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;
using System.Net.Mime;

using IAuthenticationService = Gateway.Core.Interfaces.Auth.IAuthenticationService;

namespace Gateway.Api.Controllers.Auth;

[ApiController]
[Route("/api/v1/auth/[action]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public const string RefreshTokenCookieName = "Gateway.RefreshToken";

    public AuthenticationController(IAuthenticationService authenticationService) =>
        _authenticationService = authenticationService;

    /// <summary>Authenticates the user.</summary>
    /// <param name="loginRequest">Login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    /// <remarks>
    /// The refresh token is also stored in the cookie with the name from <see cref="RefreshTokenCookieName"/>.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        Result<LoginResult, AuthenticationError> result = await _authenticationService
            .LoginAsync(loginRequest.Email, loginRequest.Password, cancellationToken);

        if (!result.IsSuccess)
            return result.Error.AsProblemDetails(this);

        switch (result.Value!)
        {
            case MfaVerificationRequired mfaVerificationRequired:
                return Ok(mfaVerificationRequired.Metadata.MapToMfaRequiredResponse());
            case LoginSucceeded loginSucceeded:
            {
                AppendRefreshTokenCookieToResponse(loginSucceeded.TokenSession.RefreshToken, loginSucceeded.TokenSession.ExpiresIn);
                return Ok(loginSucceeded.TokenSession.MapToAuthResponse());
            }
            default:
                throw new UnreachableException($"Unexpected login success result: {result.Value?.GetType().FullName}");
        }
    }

    /// <summary>Verifies the MFA.</summary>
    /// <param name="verifyMfaRequest">MFA verification request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    /// <remarks>
    /// The refresh token is also stored in the cookie with the name from <see cref="RefreshTokenCookieName"/>.
    /// </remarks>
    [HttpPost]
    [ActionName("verify/mfa")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> VerifyMfa(VerifyMfaRequest verifyMfaRequest, CancellationToken cancellationToken = default)
    {
        Result<AuthenticatedTokenSession, AuthenticationError> result = await _authenticationService
            .VerifyMultiFactorAsync(verifyMfaRequest.UserId, verifyMfaRequest.Code, cancellationToken);

        return result.Match(value =>
            {
                AppendRefreshTokenCookieToResponse(value.RefreshToken, value.ExpiresIn);
                return Ok(value.MapToAuthResponse());
            },
            error => error.AsProblemDetails(this));
    }

    /// <summary>Refreshes the session.</summary>
    /// <param name="refreshRequest">Refresh request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    [HttpPost]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Refresh(RefreshRequest refreshRequest, CancellationToken cancellationToken = default)
    {
        Result<AuthenticatedTokenSession, AuthenticationError> result = await _authenticationService
            .RefreshAsync(refreshRequest.RefreshToken, cancellationToken);

        return result.Match(value =>
            {
                AppendRefreshTokenCookieToResponse(value.RefreshToken, value.ExpiresIn);
                return Ok(value.MapToAuthResponse());
            },
            error => error.AsProblemDetails(this));
    }

    /// <summary>Revokes the access token.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>The access token is fetched from the <c>Authorization</c> header.</remarks>
    /// <returns></returns>
    /// <remarks>
    /// The refresh token is also stored in the cookie with the name from <see cref="RefreshTokenCookieName"/>.
    /// </remarks>
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

        Result<AuthenticationError> result = await _authenticationService
            .LogoutAsync(accessToken, cancellationToken);

        return result.Match(NoContent,
            error => error.AsProblemDetails(this));
    }

    private void AppendRefreshTokenCookieToResponse(string refreshToken, int expiresIn) =>
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromSeconds(expiresIn * 2)
        });
}

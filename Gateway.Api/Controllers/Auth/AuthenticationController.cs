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

    public AuthenticationController(IAuthenticationService authenticationService) =>
        _authenticationService = authenticationService;

    /// <summary>Authenticates the user.</summary>
    /// <param name="loginRequest">Login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    [HttpPost]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        Result<LoginResult, AuthenticationError> result = await _authenticationService
            .LoginAsync(loginRequest.Email, loginRequest.Password, cancellationToken);

        if (!result.IsSuccess)
            return result.Error.ToProblemDetails(this);

        return result.Value! switch
        {
            MfaVerificationRequired mfaVerificationRequired => Ok(mfaVerificationRequired.Metadata.ToResponse()),
            LoginSucceeded loginSucceeded => Ok(loginSucceeded.TokenSession.ToResponse()),
            _ => throw new UnreachableException($"Unexpected login success result: {result.Value?.GetType().FullName}")
        };
    }

    /// <summary>Verifies the MFA.</summary>
    /// <param name="verifyMfaRequest">MFA verification request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    [HttpPost]
    [ActionName("verify/mfa")]
    [ProducesResponseType<AuthenticatedResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> VerifyMfa(VerifyMfaRequest verifyMfaRequest, CancellationToken cancellationToken = default)
    {
        Result<AuthenticatedTokenSession, AuthenticationError> result = await _authenticationService
            .VerifyMultiFactorAsync(verifyMfaRequest.UserId, verifyMfaRequest.Code, cancellationToken);

        return result.Match(value => Ok(value.ToResponse()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Refreshes the session.</summary>
    /// <param name="refreshRequest">Refresh request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    [HttpPost]
    [ProducesResponseType<AuthenticatedResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> Refresh(RefreshRequest refreshRequest, CancellationToken cancellationToken = default)
    {
        Result<AuthenticatedTokenSession, AuthenticationError> result = await _authenticationService
            .RefreshAsync(refreshRequest.RefreshToken, cancellationToken);

        return result.Match(value => Ok(value.ToResponse()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Revokes the access token.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>The access token is fetched from the <c>Authorization</c> header.</remarks>
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

        Result<AuthenticationError> result = await _authenticationService
            .LogoutAsync(accessToken, cancellationToken);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }
}

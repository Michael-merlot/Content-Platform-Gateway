using Gateway.Api.Mappers;
using Gateway.Api.Models.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;

using IAuthenticationService = Gateway.Core.Interfaces.Auth.IAuthenticationService;

namespace Gateway.Api.Controllers.Auth;

[ApiController]
[Route("/api/v1/auth/[action]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AuthenticationController(IAuthenticationService authenticationService, IWebHostEnvironment webHostEnvironment)
    {
        _authenticationService = authenticationService;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>Authenticates the user.</summary>
    /// <param name="loginRequest">Login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens.</returns>
    /// <response code="200">A login response.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Wrong credentials provided.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Entity not found.</response>
    /// <response code="502">Bad gateway.</response>
    /// <response code="503">The service is unavailable.</response>
    [HttpPost]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson)]
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
    /// <response code="200">An authenticated response.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Wrong credentials provided.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Entity not found.</response>
    /// <response code="502">Bad gateway.</response>
    /// <response code="503">The service is unavailable.</response>
    [HttpPost]
    [ActionName("verify/mfa")]
    [ProducesResponseType<AuthenticatedResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson)]
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
    /// <response code="200">An authenticated response.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Wrong credentials provided.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Entity not found.</response>
    /// <response code="502">Bad gateway.</response>
    /// <response code="503">The service is unavailable.</response>
    [HttpPost]
    [ProducesResponseType<AuthenticatedResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson)]
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
    /// <response code="204">An operation completed successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Wrong credentials provided.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Entity not found.</response>
    /// <response code="502">Bad gateway.</response>
    /// <response code="503">The service is unavailable.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson)]
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

#if DEBUG // It's not possible to easily exclude from openapi by environment, so we exclude whole endpoint by the build configuration
    /// <summary>Issues a dev token for the user 1.</summary>
    /// <returns>The issued token.</returns>
    [HttpPost]
    [ProducesResponseType<string>(StatusCodes.Status200OK, MediaTypeNames.Text.Plain)]
    [ExcludeFromCodeCoverage]
    public IActionResult IssueDevToken()
    {
        // Additional check just in case
        if (!_webHostEnvironment.IsDevelopment())
            return NotFound();

        // This is synchronized with token validation settings in configuration before AddJwtBearer()
        SigningCredentials credentials =
            new(new SymmetricSecurityKey("do_not_use_this_key_in_production"u8.ToArray()), SecurityAlgorithms.HmacSha256);

        JwtSecurityTokenHandler tokenHandler = new();
        JwtSecurityToken? token = tokenHandler.CreateJwtSecurityToken("localhost", "localhost",
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1")]),
            expires: DateTime.UtcNow.AddYears(100),
            signingCredentials: credentials);

        return Ok(token.RawData);
    }
#endif
}

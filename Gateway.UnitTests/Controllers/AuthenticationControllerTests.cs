using Gateway.Api.Controllers.Auth;
using Gateway.Api.Mappers;
using Gateway.Api.Models.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using System.Security.Claims;

using IAuthenticationService = Gateway.Core.Interfaces.Auth.IAuthenticationService;

namespace Gateway.UnitTests.Controllers;

public sealed class AuthenticationControllerTests
{
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();
    private readonly ILogger<AuthenticationController> _logger = Substitute.For<ILogger<AuthenticationController>>();
    private readonly AuthenticationController _authenticationController;

    public AuthenticationControllerTests() =>
        _authenticationController = new AuthenticationController(_authService, _logger)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

    [Fact]
    public async Task Login_ServiceError_ReturnsProblem401()
    {
        LoginRequest loginRequest = new("user@example.com", "pwd");
        AuthenticationResult<LoginResult> serviceResult = new(null, AuthenticationError.InvalidClient, "invalid client");

        _authService.LoginAsync(loginRequest.Email, loginRequest.Password, CancellationToken.None)
            .Returns(serviceResult);

        IActionResult result = await _authenticationController.Login(loginRequest);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        problem.Value.ShouldBeOfType<ProblemDetails>();
    }

    [Fact]
    public async Task Login_MfaRequired_ReturnsMfaRequiredResponse()
    {
        const int uid = 123;
        LoginRequest loginRequest = new("user@example.com", "pwd");
        LoginResult loginData = new(true, null, new MfaVerificationMetadata(uid));

        _authService.LoginAsync(loginRequest.Email, loginRequest.Password, CancellationToken.None)
            .Returns(new AuthenticationResult<LoginResult>(loginData, AuthenticationError.None, null));

        IActionResult result = await _authenticationController.Login(loginRequest);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(new MfaRequiredResponse(uid));
    }

    [Fact]
    public async Task Login_TokensIssued_ReturnsAuthResponse()
    {
        LoginRequest loginRequest = new("user@example.com", "pwd");
        AuthenticatedTokenSession tokenSession = new("at", "rt", 3600, "Bearer");
        LoginResult loginData = new(false, tokenSession, null);

        _authService.LoginAsync(loginRequest.Email, loginRequest.Password, CancellationToken.None)
            .Returns(new AuthenticationResult<LoginResult>(loginData, AuthenticationError.None, null));

        IActionResult result = await _authenticationController.Login(loginRequest);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(tokenSession.ToResponse());
    }

    [Fact]
    public async Task Login_InvalidData_ReturnsProblem500_AndLogs()
    {
        LoginResult inconsistentLoginResult = new(true, null, null);

        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken.None)
            .Returns(new AuthenticationResult<LoginResult>(inconsistentLoginResult, AuthenticationError.None, null));

        IActionResult result = await _authenticationController.Login(new LoginRequest("user@example.com", "pwd"));

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        _logger.ReceivedWithAnyArgs(1)
            .Log(LogLevel.Error, default!, null!, null!, null!);
    }

    [Fact]
    public async Task VerifyMfa_ServiceSuccess_ReturnsAuthResponse()
    {
        VerifyMfaRequest verifyMfaRequest = new(123, "123456");
        AuthenticatedTokenSession tokenSession = new("at", "rt", 3600, "Bearer");

        _authService.VerifyMultiFactorAsync(verifyMfaRequest.UserId, verifyMfaRequest.Code, CancellationToken.None)
            .Returns(new AuthenticationResult<AuthenticatedTokenSession>(tokenSession, AuthenticationError.None, null));

        IActionResult result = await _authenticationController.VerifyMfa(verifyMfaRequest);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(tokenSession.ToResponse());
    }

    [Fact]
    public async Task VerifyMfa_ServiceError_ReturnsProblem()
    {
        VerifyMfaRequest verifyMfaRequest = new(123, "bad");

        _authService.VerifyMultiFactorAsync(verifyMfaRequest.UserId, verifyMfaRequest.Code, CancellationToken.None)
            .Returns(new AuthenticationResult<AuthenticatedTokenSession>(null, AuthenticationError.InvalidGrant, "bad code"));

        IActionResult result = await _authenticationController.VerifyMfa(verifyMfaRequest);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Refresh_ServiceSuccess_ReturnsAuthResponse()
    {
        RefreshRequest refreshRequest = new("rt");
        AuthenticatedTokenSession tokenSession = new("at2", "rt2", 3600, "Bearer");

        _authService.RefreshAsync(refreshRequest.RefreshToken, CancellationToken.None)
            .Returns(new AuthenticationResult<AuthenticatedTokenSession>(tokenSession, AuthenticationError.None, null));

        IActionResult result = await _authenticationController.Refresh(refreshRequest);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(tokenSession.ToResponse());
    }

    [Fact]
    public async Task Refresh_ServiceError_ReturnsProblem()
    {
        RefreshRequest refreshRequest = new("rt");

        _authService.RefreshAsync(refreshRequest.RefreshToken, CancellationToken.None)
            .Returns(new AuthenticationResult<AuthenticatedTokenSession>(null, AuthenticationError.InvalidGrant, "revoked"));

        IActionResult result = await _authenticationController.Refresh(refreshRequest);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Logout_NoAccessToken_ReturnsUnauthorized()
    {
        _authenticationController.ControllerContext.HttpContext = BuildHttpContext(null);

        IActionResult result = await _authenticationController.Logout();

        result.ShouldBeOfType<UnauthorizedResult>();
        _ = _authService.DidNotReceive().LogoutAsync(Arg.Any<string>(), CancellationToken.None);
    }

    [Fact]
    public async Task Logout_ServiceSuccess_ReturnsNoContent()
    {
        _authenticationController.ControllerContext.HttpContext = BuildHttpContext("at");

        _authService.LogoutAsync("at", CancellationToken.None)
            .Returns(new AuthenticationResult(AuthenticationError.None, null));

        IActionResult result = await _authenticationController.Logout();

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Logout_ServiceError_ReturnsProblem()
    {
        _authenticationController.ControllerContext.HttpContext = BuildHttpContext("at");

        _authService.LogoutAsync("at", CancellationToken.None)
            .Returns(new AuthenticationResult(AuthenticationError.InvalidGrant, "expired"));

        IActionResult result = await _authenticationController.Logout();

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    private static DefaultHttpContext BuildHttpContext(string? accessToken)
    {
        Microsoft.AspNetCore.Authentication.IAuthenticationService? msAuthService =
            Substitute.For<Microsoft.AspNetCore.Authentication.IAuthenticationService>();

        AuthenticationProperties authProperties = new();
        if (accessToken is not null)
        {
            authProperties.StoreTokens([
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = accessToken
                }
            ]);
        }

        AuthenticationTicket authTicket = new(new ClaimsPrincipal(new ClaimsIdentity()),
            authProperties,
            JwtBearerDefaults.AuthenticationScheme);

        msAuthService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Success(authTicket));

        ServiceCollection services = [];
        services.AddSingleton(msAuthService);
        services.AddMvcCore();
        ServiceProvider provider = services.BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = provider };
    }
}

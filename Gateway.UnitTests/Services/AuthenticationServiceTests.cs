using Gateway.Core.Interfaces.Clients;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;
using Gateway.Core.Services.Auth;

using NSubstitute;

using Shouldly;

namespace Gateway.UnitTests.Services;

public sealed class AuthenticationServiceTests
{
    private readonly ILaravelApiClient _apiClient = Substitute.For<ILaravelApiClient>();
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests() =>
        _service = new AuthenticationService(_apiClient);

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_LoginSucceeded_ReturnsTokenSession()
    {
        const string email = "user@example.com";
        const string password = "password123";
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");
        LoginSucceeded expectedLoginResult = new(expectedTokenSession);

        _apiClient.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(expectedLoginResult);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<LoginSucceeded>();
        LoginSucceeded loginSucceeded = (LoginSucceeded)result.Value!;
        loginSucceeded.TokenSession.ShouldBe(expectedTokenSession);

        await _apiClient.Received(1).LoginAsync(email, password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_MfaRequired_ReturnsMfaVerificationRequired()
    {
        const string email = "user@example.com";
        const string password = "password123";
        const int userId = 123;
        MfaVerificationMetadata expectedMfaMetadata = new(userId);
        MfaVerificationRequired expectedLoginResult = new(expectedMfaMetadata);

        _apiClient.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(expectedLoginResult);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<MfaVerificationRequired>();
        MfaVerificationRequired mfaRequired = (MfaVerificationRequired)result.Value!;
        mfaRequired.Metadata.UserId.ShouldBe(userId);

        await _apiClient.Received(1).LoginAsync(email, password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_EmailTooLong_ReturnsInvalidRequest()
    {
        string longEmail = new string('a', 257) + "@example.com";
        const string password = "password123";

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(longEmail, password);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.InvalidRequest);

        await _apiClient.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_PasswordTooLong_ReturnsInvalidRequest()
    {
        const string email = "user@example.com";
        string longPassword = new('a', 101);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, longPassword);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.InvalidRequest);

        await _apiClient.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_EmailExactlyMaxLength_CallsApiClient()
    {
        string email = new('a', 256);
        const string password = "password123";
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");
        LoginSucceeded expectedLoginResult = new(expectedTokenSession);

        _apiClient.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(expectedLoginResult);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).LoginAsync(email, password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_PasswordExactlyMaxLength_CallsApiClient()
    {
        const string email = "user@example.com";
        string password = new('a', 100);
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");
        LoginSucceeded expectedLoginResult = new(expectedTokenSession);

        _apiClient.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(expectedLoginResult);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).LoginAsync(email, password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_ApiClientError_ReturnsError()
    {
        const string email = "user@example.com";
        const string password = "wrongpassword";

        _apiClient.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(AuthenticationError.WrongCredentials);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.WrongCredentials);

        await _apiClient.Received(1).LoginAsync(email, password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_WithCancellationToken_PassesToApiClient()
    {
        const string email = "user@example.com";
        const string password = "password123";
        CancellationToken cancellationToken = new(true);
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");
        LoginSucceeded expectedLoginResult = new(expectedTokenSession);

        _apiClient.LoginAsync(email, password, cancellationToken)
            .Returns(expectedLoginResult);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).LoginAsync(email, password, cancellationToken);
    }

    #endregion

    #region VerifyMultiFactorAsync Tests

    [Fact]
    public async Task VerifyMultiFactorAsync_ValidCode_ReturnsTokenSession()
    {
        const int userId = 123;
        const string code = "123456";
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");

        _apiClient.VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>())
            .Returns(expectedTokenSession);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.VerifyMultiFactorAsync(userId, code);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedTokenSession);

        await _apiClient.Received(1).VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyMultiFactorAsync_CodeTooLong_ReturnsInvalidRequest()
    {
        const int userId = 123;
        string longCode = new('1', 11);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.VerifyMultiFactorAsync(userId, longCode);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.InvalidRequest);

        await _apiClient.DidNotReceive().VerifyMultiFactorAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyMultiFactorAsync_CodeExactlyMaxLength_CallsApiClient()
    {
        const int userId = 123;
        string code = new('1', 10);
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");

        _apiClient.VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>())
            .Returns(expectedTokenSession);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.VerifyMultiFactorAsync(userId, code);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyMultiFactorAsync_ApiClientError_ReturnsError()
    {
        const int userId = 123;
        const string code = "wrong";

        _apiClient.VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>())
            .Returns(AuthenticationError.IncorrectMfaCode);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.VerifyMultiFactorAsync(userId, code);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.IncorrectMfaCode);

        await _apiClient.Received(1).VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyMultiFactorAsync_WithCancellationToken_PassesToApiClient()
    {
        const int userId = 123;
        const string code = "123456";
        CancellationToken cancellationToken = new(true);
        AuthenticatedTokenSession expectedTokenSession = new("access_token", "refresh_token", 3600, "Bearer");

        _apiClient.VerifyMultiFactorAsync(userId, code, cancellationToken)
            .Returns(expectedTokenSession);

        Result<AuthenticatedTokenSession, AuthenticationError> result =
            await _service.VerifyMultiFactorAsync(userId, code, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).VerifyMultiFactorAsync(userId, code, cancellationToken);
    }

    #endregion

    #region RefreshAsync Tests

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsTokenSession()
    {
        const string refreshToken = "valid_refresh_token";
        AuthenticatedTokenSession expectedTokenSession = new("new_access_token", "new_refresh_token", 3600, "Bearer");

        _apiClient.RefreshAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(expectedTokenSession);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.RefreshAsync(refreshToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedTokenSession);

        await _apiClient.Received(1).RefreshAsync(refreshToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_TokenTooLong_ReturnsInvalidRequest()
    {
        string longToken = new('a', 2001);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.RefreshAsync(longToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.InvalidRequest);

        await _apiClient.DidNotReceive().RefreshAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_TokenExactlyMaxLength_CallsApiClient()
    {
        string refreshToken = new('a', 2000);
        AuthenticatedTokenSession expectedTokenSession = new("new_access_token", "new_refresh_token", 3600, "Bearer");

        _apiClient.RefreshAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(expectedTokenSession);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.RefreshAsync(refreshToken);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).RefreshAsync(refreshToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_ApiClientError_ReturnsError()
    {
        const string refreshToken = "invalid_token";

        _apiClient.RefreshAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(AuthenticationError.InvalidGrant);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.RefreshAsync(refreshToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.InvalidGrant);

        await _apiClient.Received(1).RefreshAsync(refreshToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshAsync_WithCancellationToken_PassesToApiClient()
    {
        const string refreshToken = "valid_refresh_token";
        CancellationToken cancellationToken = new(true);
        AuthenticatedTokenSession expectedTokenSession = new("new_access_token", "new_refresh_token", 3600, "Bearer");

        _apiClient.RefreshAsync(refreshToken, cancellationToken)
            .Returns(expectedTokenSession);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.RefreshAsync(refreshToken, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).RefreshAsync(refreshToken, cancellationToken);
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_ValidToken_ReturnsSuccess()
    {
        const string accessToken = "valid_access_token";

        _apiClient.LogoutAsync(accessToken, Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticationError>.Success());

        Result<AuthenticationError> result = await _service.LogoutAsync(accessToken);

        result.IsSuccess.ShouldBeTrue();

        await _apiClient.Received(1).LogoutAsync(accessToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_ApiClientError_ReturnsError()
    {
        const string accessToken = "invalid_token";

        _apiClient.LogoutAsync(accessToken, Arg.Any<CancellationToken>())
            .Returns(AuthenticationError.InvalidClient);

        Result<AuthenticationError> result = await _service.LogoutAsync(accessToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthenticationError.InvalidClient);

        await _apiClient.Received(1).LogoutAsync(accessToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WithCancellationToken_PassesToApiClient()
    {
        const string accessToken = "valid_access_token";
        CancellationToken cancellationToken = new(true);

        _apiClient.LogoutAsync(accessToken, cancellationToken)
            .Returns(Result<AuthenticationError>.Success());

        Result<AuthenticationError> result = await _service.LogoutAsync(accessToken, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).LogoutAsync(accessToken, cancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short_token")]
    [InlineData("very_long_token_that_is_still_valid")]
    public async Task LogoutAsync_VariousTokenLengths_CallsApiClient(string accessToken)
    {
        _apiClient.LogoutAsync(accessToken, Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticationError>.Success());

        Result<AuthenticationError> result = await _service.LogoutAsync(accessToken);

        result.IsSuccess.ShouldBeTrue();
        await _apiClient.Received(1).LogoutAsync(accessToken, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Theory]
    [InlineData(AuthenticationError.WrongCredentials)]
    [InlineData(AuthenticationError.InvalidRequest)]
    [InlineData(AuthenticationError.ServerError)]
    [InlineData(AuthenticationError.NetworkError)]
    [InlineData(AuthenticationError.Forbidden)]
    [InlineData(AuthenticationError.NotFound)]
    public async Task LoginAsync_VariousErrors_ReturnsCorrectError(AuthenticationError expectedError)
    {
        const string email = "user@example.com";
        const string password = "password123";

        _apiClient.LoginAsync(email, password, Arg.Any<CancellationToken>())
            .Returns(expectedError);

        Result<LoginResult, AuthenticationError> result = await _service.LoginAsync(email, password);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(AuthenticationError.IncorrectMfaCode)]
    [InlineData(AuthenticationError.InvalidRequest)]
    [InlineData(AuthenticationError.ServerError)]
    [InlineData(AuthenticationError.NetworkError)]
    [InlineData(AuthenticationError.Forbidden)]
    public async Task VerifyMultiFactorAsync_VariousErrors_ReturnsCorrectError(AuthenticationError expectedError)
    {
        const int userId = 123;
        const string code = "123456";

        _apiClient.VerifyMultiFactorAsync(userId, code, Arg.Any<CancellationToken>())
            .Returns(expectedError);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.VerifyMultiFactorAsync(userId, code);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(AuthenticationError.InvalidGrant)]
    [InlineData(AuthenticationError.InvalidClient)]
    [InlineData(AuthenticationError.ServerError)]
    [InlineData(AuthenticationError.NetworkError)]
    public async Task RefreshAsync_VariousErrors_ReturnsCorrectError(AuthenticationError expectedError)
    {
        const string refreshToken = "refresh_token";

        _apiClient.RefreshAsync(refreshToken, Arg.Any<CancellationToken>())
            .Returns(expectedError);

        Result<AuthenticatedTokenSession, AuthenticationError> result = await _service.RefreshAsync(refreshToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(AuthenticationError.InvalidClient)]
    [InlineData(AuthenticationError.ServerError)]
    [InlineData(AuthenticationError.NetworkError)]
    [InlineData(AuthenticationError.Forbidden)]
    public async Task LogoutAsync_VariousErrors_ReturnsCorrectError(AuthenticationError expectedError)
    {
        const string accessToken = "access_token";

        _apiClient.LogoutAsync(accessToken, Arg.Any<CancellationToken>())
            .Returns(expectedError);

        Result<AuthenticationError> result = await _service.LogoutAsync(accessToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(expectedError);
    }

    #endregion
}

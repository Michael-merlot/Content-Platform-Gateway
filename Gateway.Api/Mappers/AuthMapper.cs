using Gateway.Api.Models.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Mappers;

public static class AuthMapper
{
    /// <summary>Maps <see cref="AuthTokenSession"/> to <see cref="AuthResponse"/></summary>
    /// <param name="authTokenSession">The <see cref="AuthTokenSession"/> to map</param>
    /// <returns>Mapped value</returns>
    public static AuthResponse MapToAuthResponse(this AuthTokenSession authTokenSession) =>
        new(authTokenSession.AccessToken, authTokenSession.RefreshToken, authTokenSession.ExpiresIn, authTokenSession.TokenType);

    /// <summary>Constructs <see cref="IActionResult"/> with problem details from the <see cref="AuthResult"/></summary>
    /// <param name="result">The <see cref="AuthResult"/> to construct from</param>
    /// <param name="controller">The controller from which the problem details will be constructed</param>
    /// <returns>Problem details action result</returns>
    public static IActionResult AsProblemDetails(this AuthResult result, ControllerBase controller) =>
        controller.Problem(statusCode: result.Error.MapToHttpStatus(),
            title: result.Error.ToString(),
            detail: result.ErrorDescription);

    /// <summary>Maps <see cref="AuthError"/> to <see cref="int"/> HTTP status code</summary>
    /// <param name="error">The <see cref="AuthError"/> to map</param>
    /// <returns>HTTP status code</returns>
    private static int MapToHttpStatus(this AuthError error) =>
        error switch
        {
            AuthError.None => StatusCodes.Status200OK,
            AuthError.InvalidRequest => StatusCodes.Status400BadRequest,
            AuthError.InvalidClient
                or AuthError.InvalidGrant => StatusCodes.Status401Unauthorized,
            AuthError.Forbidden => StatusCodes.Status403Forbidden,
            AuthError.NotFound => StatusCodes.Status404NotFound,
            AuthError.ServerError => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status503ServiceUnavailable
        };
}

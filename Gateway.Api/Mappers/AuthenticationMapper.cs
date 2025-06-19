using Gateway.Api.Models.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Mappers;

internal static class AuthenticationMapper
{
    /// <summary>Maps <see cref="AuthenticatedTokenSession"/> to <see cref="AuthenticatedResponse"/>.</summary>
    /// <param name="authenticatedTokenSession">The <see cref="AuthenticatedTokenSession"/> to map.</param>
    /// <returns>Mapped value.</returns>
    public static AuthenticatedResponse ToResponse(this AuthenticatedTokenSession authenticatedTokenSession) =>
        new(authenticatedTokenSession.AccessToken, authenticatedTokenSession.RefreshToken, authenticatedTokenSession.ExpiresIn,
            authenticatedTokenSession.TokenType);

    /// <summary>Maps <see cref="MfaVerificationMetadata"/> to <see cref="MfaRequiredResponse"/>.</summary>
    /// <param name="mfaVerificationMetadata">The <see cref="MfaVerificationMetadata"/> to map.</param>
    /// <returns>Mapped value.</returns>
    public static MfaRequiredResponse ToResponse(this MfaVerificationMetadata mfaVerificationMetadata) =>
        new(mfaVerificationMetadata.UserId);

    /// <summary>
    /// Constructs <see cref="IActionResult"/> with problem details from the <see cref="AuthenticationError"/>.
    /// </summary>
    /// <param name="error">The <see cref="AuthenticationError"/> to construct from.</param>
    /// <param name="controller">The controller from which the problem details will be constructed.</param>
    /// <returns>Problem details action result.</returns>
    public static IActionResult ToProblemDetails(this AuthenticationError error, ControllerBase controller) =>
        controller.Problem(statusCode: error.ToHttpStatus(),
            title: error.ToString());

    /// <summary>Maps <see cref="AuthenticationError"/> to <see cref="int"/> HTTP status code.</summary>
    /// <param name="error">The <see cref="AuthenticationError"/> to map.</param>
    /// <returns>HTTP status code.</returns>
    private static int ToHttpStatus(this AuthenticationError error) =>
        error switch
        {
            AuthenticationError.None => StatusCodes.Status200OK,
            AuthenticationError.InvalidRequest => StatusCodes.Status400BadRequest,
            AuthenticationError.InvalidClient
                or AuthenticationError.InvalidGrant => StatusCodes.Status401Unauthorized,
            AuthenticationError.Forbidden => StatusCodes.Status403Forbidden,
            AuthenticationError.NotFound => StatusCodes.Status404NotFound,
            AuthenticationError.ServerError => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status503ServiceUnavailable
        };
}

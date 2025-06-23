namespace Gateway.Api.Models.Auth;

/// <summary>An endpoint represented by the framework itself.</summary>
/// <param name="Controller">The controller of the endpoint.</param>
/// <param name="Action">The action of the endpoint.</param>
/// <param name="HttpMethod">The HttpMethod of the endpoint.</param>
public sealed record FrameworkEndpointDto(
    string Controller,
    string Action,
    string HttpMethod
);

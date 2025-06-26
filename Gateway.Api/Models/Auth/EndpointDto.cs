namespace Gateway.Api.Models.Auth;

/// <summary>Represents an endpoint.</summary>
/// <param name="Id">The unique identifier of the endpoint.</param>
/// <param name="Controller">The controller of the endpoint.</param>
/// <param name="Action">The action of the endpoint.</param>
/// <param name="HttpMethod">The HTTP method of the endpoint.</param>
public sealed record EndpointDto(
    long Id,
    string Controller,
    string Action,
    string HttpMethod
);

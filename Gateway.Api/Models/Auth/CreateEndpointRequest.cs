using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models.Auth;

/// <summary>Represents a request to create an endpoint.</summary>
/// <param name="Controller">The controller of an endpoint.</param>
/// <param name="Action">The action of an endpoint.</param>
/// <param name="HttpMethod">The HTTP method of an endpoint.</param>
public sealed record CreateEndpointRequest(
    [Required] [MaxLength(100)] string Controller,
    [Required] [MaxLength(100)] string Action,
    [Required] [MaxLength(100)] string HttpMethod
);

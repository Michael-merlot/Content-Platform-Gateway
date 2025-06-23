namespace Gateway.Api.Models.Auth;

/// <summary>The endpoint collection response.</summary>
/// <param name="Endpoints">The collection of endpoints.</param>
public sealed record EndpointCollectionResponse(IEnumerable<EndpointDto> Endpoints);

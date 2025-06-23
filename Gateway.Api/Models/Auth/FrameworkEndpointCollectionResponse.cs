namespace Gateway.Api.Models.Auth;

/// <summary>The framework endpoint collection response.</summary>
/// <param name="Endpoints">The collection of framework endpoints.</param>
public sealed record FrameworkEndpointCollectionResponse(IEnumerable<FrameworkEndpointDto> Endpoints);

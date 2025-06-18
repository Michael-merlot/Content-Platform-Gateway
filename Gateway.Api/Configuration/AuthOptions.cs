namespace Gateway.Api.Options;

/// <summary>Authentication options.</summary>
public class AuthOptions
{
    /// <summary>Public key in PEM format to verify JWTs.</summary>
    public string PublicKeyPem { get; set; } = String.Empty;

    /// <summary>Authority to fetch JWKS from.</summary>
    public string Authority { get; set; } = String.Empty;
}

namespace Gateway.Api.Options;

/// <summary>Authentication options</summary>
public class AuthOptions
{
    /// <summary>Public key in PEM format to verify JWTs</summary>
    public string PublicKeyPem { get; set; } = String.Empty;

    /// <summary>Issuer claim to verify on JWTs</summary>
    public string Issuer { get; set; } = String.Empty;

    /// <summary>Audience claim to verify on JWTs</summary>
    public string Audience { get; set; } = String.Empty;
}

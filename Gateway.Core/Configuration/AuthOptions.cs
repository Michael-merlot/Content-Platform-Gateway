namespace Gateway.Core.Configuration;

/// <summary>Authentication options.</summary>
public class AuthOptions
{
    /// <summary>Authority to fetch JWKS from.</summary>
    public string Authority { get; set; } = String.Empty;
}

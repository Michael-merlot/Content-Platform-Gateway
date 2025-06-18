using System.ComponentModel.DataAnnotations;

namespace Gateway.Core.Configuration;

/// <summary>Authentication options.</summary>
public class AuthOptions
{
    /// <summary>The name of the section for the options.</summary>
    public const string SectionName = "Auth";

    /// <summary>Authority to fetch JWKS from.</summary>
    [Required]
    public required Uri Authority { get; set; }
}

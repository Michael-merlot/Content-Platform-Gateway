namespace Gateway.Core.Models.Auth;

/// <summary>Defines constants for the additional known claim types that can be assigned to a subject.</summary>
public static class ExtraClaimTypes
{
    /// <summary>Represents the claim type for permissions.</summary>
    public const string Permission = "permission";

    /// <summary>Represents the claim type for specifying that the user is an admin.</summary>
    public const string IsAdmin = "is_admin";
}

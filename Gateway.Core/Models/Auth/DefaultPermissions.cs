namespace Gateway.Core.Models.Auth;

/// <summary>Default permissions.</summary>
public static class DefaultPermissions
{
    /// <summary>Example permission name.</summary>
    public const string DoNothing = "do.nothing";

    /// <summary>Gets the collection of all default permissions.</summary>
    /// <returns>The collection of all default permissions.</returns>
    /// <remarks>Used by the migrations to determine whether or not the seeded permissions need to be updated.</remarks>
    public static IEnumerable<Permission> GetPermissions() =>
    [
        new(1, DoNothing, "Do nothing") // Example permission
    ];
}

using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <inheritdoc/>
public class EndpointRepository : IEndpointRepository
{
    private readonly AuthDbContext _authDbContext;

    public EndpointRepository(AuthDbContext authDbContext) =>
        _authDbContext = authDbContext;

    /// <inheritdoc/>
    public async Task<IEnumerable<Permission>> GetRequiredPermissionsAsync(string controller, string action, string httpMethod) =>
        await _authDbContext.Endpoints
            .AsNoTracking()
            .Where(x => x.Controller == controller && x.Action == action && x.HttpMethod == httpMethod)
            .SelectMany(x => x.EndpointPermissions.Select(x => x.Permission))
            .ToListAsync();
}

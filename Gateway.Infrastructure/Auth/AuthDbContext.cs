using Gateway.Core.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Auth;

/// <summary>Represents the <see cref="DbContext"/> for managing the auth.</summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    /// <summary>Represents the Roles table in the database.</summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>Represents the Permissions table in the database.</summary>
    public DbSet<Permission> Permissions { get; set; }

    /// <summary>Represents the join table between users and roles.</summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>Represents the join table between roles and permissions.</summary>
    public DbSet<RolePermission> AuthRolePermissions { get; set; }

    /// <summary>Represents the Endpoints table in the database.</summary>
    public DbSet<Endpoint> Endpoints { get; set; }

    /// <summary>Represents the join table between endpoints and permissions.</summary>
    public DbSet<EndpointPermission> EndpointsPermissions { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => new
            {
                x.UserId,
                x.RoleId
            });
            e.HasOne(x => x.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(x => x.RoleId);
        });

        builder.Entity<Role>(e =>
        {
            e.ToTable("auth_roles");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name)
                .IsUnique();
        });

        builder.Entity<Permission>(e =>
        {
            e.ToTable("auth_permissions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name)
                .IsUnique();
        });

        builder.Entity<RolePermission>(e =>
        {
            e.ToTable("auth_role_permissions");
            e.HasKey(x => new
            {
                x.RoleId,
                x.PermissionId
            });
            e.HasOne(x => x.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(x => x.PermissionId);
        });

        builder.Entity<Endpoint>(e =>
        {
            e.ToTable("endpoints");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new
                {
                    x.Controller,
                    x.Action,
                    x.HttpMethod
                })
                .IsUnique();
        });

        builder.Entity<EndpointPermission>(e =>
        {
            e.ToTable("endpoints_permissions");
            e.HasKey(x => new
            {
                x.EndpointId,
                x.PermissionId
            });
            e.HasOne(x => x.Endpoint)
                .WithMany(e => e.EndpointPermissions)
                .HasForeignKey(x => x.EndpointId);
            e.HasOne(x => x.Permission)
                .WithMany(p => p.EndpointPermissions)
                .HasForeignKey(x => x.PermissionId);
        });
    }
}

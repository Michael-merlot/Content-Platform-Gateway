using EntityFramework.Exceptions.PostgreSQL;

using Gateway.Core.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Persistence.Auth;

/// <summary>Represents the <see cref="DbContext"/> for managing the auth.</summary>
public class AuthDbContext : DbContext
{
    /// <summary>The name of the connection string.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>Represents the Roles table in the database.</summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>Represents the Permissions table in the database.</summary>
    public DbSet<Permission> Permissions { get; set; }

    /// <summary>Represents the join table between users and roles.</summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>Represents the join table between roles and permissions.</summary>
    public DbSet<RolePermission> RolePermissions { get; set; }

    /// <summary>Represents the Endpoints table in the database.</summary>
    public DbSet<Endpoint> Endpoints { get; set; }

    /// <summary>Represents the join table between endpoints and permissions.</summary>
    public DbSet<EndpointPermission> EndpointPermissions { get; set; }

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseExceptionProcessor();
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("auth");

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
            e.ToTable("roles");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name)
                .IsUnique();
            e.Property(x => x.Name)
                .HasMaxLength(100);
        });

        builder.Entity<Permission>(e =>
        {
            e.ToTable("permissions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name)
                .IsUnique();
            e.Property(x => x.Name)
                .HasMaxLength(100);
            e.Property(x => x.Description)
                .HasMaxLength(500);
        });

        builder.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions");
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
            e.Property(x => x.Controller)
                .HasMaxLength(100);
            e.Property(x => x.Action)
                .HasMaxLength(100);
            e.Property(x => x.HttpMethod)
                .HasMaxLength(100);
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

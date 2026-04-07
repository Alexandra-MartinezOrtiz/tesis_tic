using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Scaffolded;

/// <summary>
/// Modelo EF alineado al DDL database-first en <c>backend/sql/ticfis_identity/001_init.sql</c>.
/// Para regenerar desde PostgreSQL: <c>backend/tools/scaffold-from-db.ps1</c> (salida en <c>Persistence/Scaffold/EfGenerated</c>).
/// </summary>
public class TicfisIdentityDbContext : DbContext
{
    public TicfisIdentityDbContext(DbContextOptions<TicfisIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRole> UsuarioRoles => Set<UsuarioRole>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<TokensRevocado> TokensRevocados => Set<TokensRevocado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Activo);
            e.HasMany(x => x.UsuarioRoles).WithOne(x => x.Usuario).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.TokensRevocados).WithOne(x => x.Usuario).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("roles");
            e.HasIndex(x => x.Nombre).IsUnique();
            e.HasMany(x => x.UsuarioRoles).WithOne(x => x.Rol).HasForeignKey(x => x.RolId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.RolPermisos).WithOne(x => x.Rol).HasForeignKey(x => x.RolId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Permiso>(e =>
        {
            e.ToTable("permisos");
            e.HasIndex(x => x.Codigo).IsUnique();
            e.HasMany(x => x.RolPermisos).WithOne(x => x.Permiso).HasForeignKey(x => x.PermisoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UsuarioRole>(e =>
        {
            e.ToTable("usuario_roles");
            e.HasIndex(x => x.UsuarioId);
            e.HasIndex(x => new { x.UsuarioId, x.RolId }).IsUnique();
        });

        modelBuilder.Entity<RolPermiso>(e =>
        {
            e.ToTable("rol_permisos");
            e.HasIndex(x => new { x.RolId, x.PermisoId }).IsUnique();
        });

        modelBuilder.Entity<TokensRevocado>(e =>
        {
            e.ToTable("tokens_revocados");
            e.HasIndex(x => x.TokenJti);
            e.HasIndex(x => x.UsuarioId);
        });
    }
}

using Microsoft.EntityFrameworkCore;

namespace Propuestas.Infrastructure.Persistence.Scaffolded;

/// <summary>
/// Modelo EF alineado a <c>backend/sql/ticfis_propuestas/001_init.sql</c> (database-first).
/// </summary>
public class TicfisPropuestasDbContext : DbContext
{
    public TicfisPropuestasDbContext(DbContextOptions<TicfisPropuestasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Propuesta> Propuestas => Set<Propuesta>();
    public DbSet<PropuestaEstudiante> PropuestaEstudiantes => Set<PropuestaEstudiante>();
    public DbSet<PropuestaObservacion> PropuestaObservaciones => Set<PropuestaObservacion>();
    public DbSet<PropuestaHistorialEstado> PropuestaHistorialEstados => Set<PropuestaHistorialEstado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Docente>(e =>
        {
            e.ToTable("docentes");
            e.HasIndex(x => x.UsuarioIdReferencia).IsUnique();
            e.HasMany(x => x.Propuestas).WithOne(x => x.Docente).HasForeignKey(x => x.DocenteId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Estudiante>(e =>
        {
            e.ToTable("estudiantes");
            e.HasIndex(x => x.Email);
            e.HasMany(x => x.PropuestaEstudiantes).WithOne(x => x.Estudiante).HasForeignKey(x => x.EstudianteId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Propuesta>(e =>
        {
            e.ToTable("propuestas");
            e.HasIndex(x => x.Codigo).IsUnique();
            e.HasIndex(x => x.DocenteId);
            e.HasIndex(x => x.EstadoActual);
            e.HasIndex(x => x.FechaUltimaActualizacion);
            e.HasMany(x => x.PropuestaEstudiantes).WithOne(x => x.Propuesta).HasForeignKey(x => x.PropuestaId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.PropuestaObservaciones).WithOne(x => x.Propuesta).HasForeignKey(x => x.PropuestaId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.PropuestaHistorialEstados).WithOne(x => x.Propuesta).HasForeignKey(x => x.PropuestaId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropuestaEstudiante>(e =>
        {
            e.ToTable("propuesta_estudiantes");
            e.HasIndex(x => new { x.PropuestaId, x.EstudianteId }).IsUnique();
        });

        modelBuilder.Entity<PropuestaObservacion>(e =>
        {
            e.ToTable("propuesta_observaciones");
            e.HasIndex(x => x.PropuestaId);
        });

        modelBuilder.Entity<PropuestaHistorialEstado>(e =>
        {
            e.ToTable("propuesta_historial_estados");
            e.HasIndex(x => x.PropuestaId);
        });
    }
}

using Microsoft.EntityFrameworkCore;

namespace Reportes.Infrastructure.Persistence.Scaffolded;

/// <summary>
/// Modelo EF alineado a <c>backend/sql/ticfis_reportes/001_init.sql</c> (database-first).
/// </summary>
public class TicfisReportesDbContext : DbContext
{
    public TicfisReportesDbContext(DbContextOptions<TicfisReportesDbContext> options)
        : base(options)
    {
    }

    public DbSet<PropuestaConsulta> PropuestasConsulta => Set<PropuestaConsulta>();
    public DbSet<PropuestaConsultaEstudiante> PropuestasConsultaEstudiantes => Set<PropuestaConsultaEstudiante>();
    public DbSet<ReporteGenerado> ReportesGenerados => Set<ReporteGenerado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropuestaConsulta>(e =>
        {
            e.ToTable("propuestas_consulta");
            e.HasIndex(x => x.Codigo);
            e.HasIndex(x => x.EstadoActual);
            e.HasIndex(x => x.PropuestaIdOrigen).IsUnique();
            e.HasMany(x => x.PropuestaConsultaEstudiantes).WithOne(x => x.PropuestaConsulta).HasForeignKey(x => x.PropuestaConsultaId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropuestaConsultaEstudiante>(e =>
        {
            e.ToTable("propuestas_consulta_estudiantes");
            e.HasIndex(x => x.PropuestaConsultaId);
        });

        modelBuilder.Entity<ReporteGenerado>(e =>
        {
            e.ToTable("reportes_generados");
            e.Property(x => x.ParametrosJson).HasColumnType("jsonb");
            e.HasIndex(x => x.GeneradoPorUsuarioId);
            e.HasIndex(x => x.GeneradoEn);
        });

        modelBuilder.Entity<PropuestaConsultaEstudiante>(e =>
        {
            e.ToTable("propuestas_consulta_estudiantes");
        });
    }
}

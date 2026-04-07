namespace Reportes.Infrastructure.Persistence.Scaffolded;

public class PropuestaConsulta
{
    public long Id { get; set; }
    public long PropuestaIdOrigen { get; set; }
    public string Codigo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string? DocenteNombre { get; set; }
    public string EstadoActual { get; set; } = null!;
    public DateTimeOffset FechaUltimaActualizacion { get; set; }
    public bool Disponible { get; set; }
    public ICollection<PropuestaConsultaEstudiante> PropuestaConsultaEstudiantes { get; set; } = new List<PropuestaConsultaEstudiante>();
}

public class PropuestaConsultaEstudiante
{
    public long Id { get; set; }
    public long PropuestaConsultaId { get; set; }
    public string? EstudianteNombre { get; set; }
    public string? EstudianteEmail { get; set; }
    public PropuestaConsulta PropuestaConsulta { get; set; } = null!;
}

public class ReporteGenerado
{
    public long Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string? ParametrosJson { get; set; }
    public long GeneradoPorUsuarioId { get; set; }
    public DateTimeOffset GeneradoEn { get; set; }
    public string? RutaArchivo { get; set; }
}

namespace Propuestas.Infrastructure.Persistence.Scaffolded;

public class Docente
{
    public long Id { get; set; }
    public long UsuarioIdReferencia { get; set; }
    public string? Email { get; set; }
    public string? Titulo { get; set; }
    public string? Departamento { get; set; }
    public ICollection<Propuesta> Propuestas { get; set; } = new List<Propuesta>();
}

public class Estudiante
{
    public long Id { get; set; }
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Carrera { get; set; }
    public ICollection<PropuestaEstudiante> PropuestaEstudiantes { get; set; } = new List<PropuestaEstudiante>();
}

public class Propuesta
{
    public long Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Problema { get; set; }
    public string? ObjetivoGeneral { get; set; }
    public string? Alcance { get; set; }
    public long DocenteId { get; set; }
    public string EstadoActual { get; set; } = null!;
    public DateTimeOffset? FechaEnvio { get; set; }
    public DateTimeOffset FechaUltimaActualizacion { get; set; }
    public bool Activa { get; set; }
    /// <summary>
    /// Cantidad de estudiantes propuestos al crear la propuesta (0..5).
    /// Usado por el módulo de Consultas y Reportes para calcular cupos y disponibilidad.
    /// </summary>
    public int EstudiantesPropuestos { get; set; }
    /// <summary>Carrera de la FIS seleccionada en el formulario (ej. "Software").</summary>
    public string? Carrera { get; set; }
    /// <summary>Asignaturas seleccionadas, una por línea con formato "Nombre (CÓDIGO)".</summary>
    public string? Asignaturas { get; set; }
    /// <summary>"Autorizado por" de la solicitud de participación (&lt;2 o &gt;5 estudiantes).</summary>
    public string? AutorizadoPor { get; set; }
    /// <summary>Fecha de esa autorización (texto ISO yyyy-MM-dd).</summary>
    public string? FechaAutorizacion { get; set; }
    // Sección "Recomendaciones y aprobaciones" del formulario F_AA_233A.
    public string? PresentadoPor { get; set; }
    public string? EstudiantesNombres { get; set; }
    public string? ResolucionCpgic { get; set; }
    public string? PresidenteCpgic { get; set; }
    public string? FechaAprobacion { get; set; }
    public Docente Docente { get; set; } = null!;
    public ICollection<PropuestaEstudiante> PropuestaEstudiantes { get; set; } = new List<PropuestaEstudiante>();
    public ICollection<PropuestaObservacion> PropuestaObservaciones { get; set; } = new List<PropuestaObservacion>();
    public ICollection<PropuestaHistorialEstado> PropuestaHistorialEstados { get; set; } = new List<PropuestaHistorialEstado>();
}

public class PropuestaEstudiante
{
    public long Id { get; set; }
    public long PropuestaId { get; set; }
    public long EstudianteId { get; set; }
    public DateTimeOffset FechaAsignacion { get; set; }
    public Propuesta Propuesta { get; set; } = null!;
    public Estudiante Estudiante { get; set; } = null!;
}

public class PropuestaObservacion
{
    public long Id { get; set; }
    public long PropuestaId { get; set; }
    public string Observacion { get; set; } = null!;
    public long CreadoPorUsuarioId { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public Propuesta Propuesta { get; set; } = null!;
}

public class PropuestaHistorialEstado
{
    public long Id { get; set; }
    public long PropuestaId { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = null!;
    public string? Comentario { get; set; }
    public long CambiadoPorUsuarioId { get; set; }
    public DateTimeOffset CambiadoEn { get; set; }
    public Propuesta Propuesta { get; set; } = null!;
}

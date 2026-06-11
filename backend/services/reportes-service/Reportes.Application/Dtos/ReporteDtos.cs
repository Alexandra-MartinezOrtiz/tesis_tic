namespace Reportes.Application.Dtos;

public record PropuestaReporteItemDto(
    long Id,
    string Codigo,
    string Titulo,
    string EstadoActual,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    string? DocenteEmail);

public record EstudianteReporteDto(
    string NombreCompleto,
    string Email,
    DateTimeOffset FechaAsignacion);

public record PropuestaReporteDetalleDto(
    long Id,
    string Codigo,
    string Titulo,
    string? Descripcion,
    string? Problema,
    string? ObjetivoGeneral,
    string? Alcance,
    long DocenteId,
    long DocenteUsuarioIdReferencia,
    string EstadoActual,
    DateTimeOffset? FechaEnvio,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    IReadOnlyList<EstudianteReporteDto> Estudiantes);

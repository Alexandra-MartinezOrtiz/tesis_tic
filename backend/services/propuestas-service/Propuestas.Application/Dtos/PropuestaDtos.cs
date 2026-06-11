namespace Propuestas.Application.Dtos;

public record PropuestaListItemDto(
    long Id,
    string Codigo,
    string Titulo,
    string EstadoActual,
    DateTimeOffset FechaUltimaActualizacion,
    bool Activa,
    string? DocenteEmail);

public record PropuestaDetailDto(
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
    IReadOnlyList<EstudianteAsignadoDto> Estudiantes,
    IReadOnlyList<ObservacionDto> Observaciones);

public record EstudianteAsignadoDto(long EstudianteId, string NombreCompleto, string Email, DateTimeOffset FechaAsignacion);
public record ObservacionDto(long Id, string Texto, long CreadoPorUsuarioId, DateTimeOffset CreadoEn);

public record CreatePropuestaRequest(
    string Codigo,
    string Titulo,
    string? Descripcion,
    string? Problema,
    string? ObjetivoGeneral,
    string? Alcance);

public record UpdatePropuestaRequest(
    string Titulo,
    string? Descripcion,
    string? Problema,
    string? ObjetivoGeneral,
    string? Alcance);

public record AsignarEstudiantesRequest(IReadOnlyList<long> EstudianteIds);

public record TransicionRequest(string? Comentario);

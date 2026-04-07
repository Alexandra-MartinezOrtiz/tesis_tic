using Propuestas.Application.Dtos;

namespace Propuestas.Application.Abstractions;

public interface IPropuestaRepository
{
    Task<long> EnsureDocenteAsync(long usuarioIdReferencia, CancellationToken cancellationToken = default);
    Task<bool> CodigoExisteAsync(string codigo, long? excludePropuestaId, CancellationToken cancellationToken = default);
    Task<long> InsertPropuestaAsync(CreatePropuestaRequest request, long docenteId, long actorUsuarioId, CancellationToken cancellationToken = default);
    Task<PropuestaDetailDto?> GetDetailAsync(long propuestaId, CancellationToken cancellationToken = default);
    Task UpdatePropuestaBasicaAsync(long propuestaId, UpdatePropuestaRequest request, CancellationToken cancellationToken = default);
    Task SetEstadoAsync(long propuestaId, string estadoNuevo, DateTimeOffset? fechaEnvio, CancellationToken cancellationToken = default);
    Task AddHistorialAsync(long propuestaId, string? estadoAnterior, string estadoNuevo, long cambiadoPorUsuarioId, string? comentario, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropuestaListItemDto>> ListAsync(long? docenteId, bool verTodas, string? estado, int skip, int take, CancellationToken cancellationToken = default);
    Task<long?> GetDocenteIdPorUsuarioAsync(long usuarioIdReferencia, CancellationToken cancellationToken = default);
    Task<bool> PropuestaPerteneceADocenteAsync(long propuestaId, long docenteId, CancellationToken cancellationToken = default);
    Task<bool> EstudiantesExistenAsync(IReadOnlyList<long> estudianteIds, CancellationToken cancellationToken = default);
    Task LimpiarAsignacionesAsync(long propuestaId, CancellationToken cancellationToken = default);
    Task AsignarEstudiantesAsync(long propuestaId, IReadOnlyList<long> estudianteIds, CancellationToken cancellationToken = default);
}

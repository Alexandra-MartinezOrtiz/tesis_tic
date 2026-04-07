using Propuestas.Application.Dtos;

namespace Propuestas.Application.Abstractions;

public interface IPropuestaService
{
    Task<PropuestaDetailDto> CrearAsync(long usuarioId, IReadOnlyList<string> roles, CreatePropuestaRequest request, CancellationToken cancellationToken = default);
    Task<PropuestaDetailDto> ActualizarAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, UpdatePropuestaRequest request, CancellationToken cancellationToken = default);
    Task<PropuestaDetailDto> ObtenerAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropuestaListItemDto>> ListarAsync(long usuarioId, IReadOnlyList<string> roles, string? estado, int page, int pageSize, CancellationToken cancellationToken = default);
    Task EnviarRevisionAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, CancellationToken cancellationToken = default);
    Task AprobarAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, TransicionRequest? request, CancellationToken cancellationToken = default);
    Task RechazarAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, TransicionRequest? request, CancellationToken cancellationToken = default);
    Task MarcarPendienteAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, TransicionRequest? request, CancellationToken cancellationToken = default);
    Task AsignarEstudiantesAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, AsignarEstudiantesRequest request, CancellationToken cancellationToken = default);
}

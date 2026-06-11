using Propuestas.Application.Abstractions;
using Propuestas.Application.Domain;
using Propuestas.Application.Dtos;

namespace Propuestas.Application.Services;

public class PropuestaService : IPropuestaService
{
    private readonly IPropuestaRepository _repo;

    public PropuestaService(IPropuestaRepository repo)
    {
        _repo = repo;
    }

    private static bool EsAdmin(IReadOnlyList<string> roles) => roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
    private static bool EsCpgic(IReadOnlyList<string> roles) => roles.Contains("CPGIC", StringComparer.OrdinalIgnoreCase) || EsAdmin(roles);
    private static bool EsDocente(IReadOnlyList<string> roles) => roles.Contains("Docente", StringComparer.OrdinalIgnoreCase) || EsAdmin(roles);

    public async Task<PropuestaDetailDto> CrearAsync(long usuarioId, IReadOnlyList<string> roles, CreatePropuestaRequest request, string? docenteEmail = null, CancellationToken cancellationToken = default)
    {
        if (!EsDocente(roles))
            throw new UnauthorizedAccessException("Se requiere rol Docente.");
        if (await _repo.CodigoExisteAsync(request.Codigo, null, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("El código de propuesta ya existe.");
        var docenteId = await _repo.EnsureDocenteAsync(usuarioId, docenteEmail, cancellationToken).ConfigureAwait(false);
        var id = await _repo.InsertPropuestaAsync(request, docenteId, usuarioId, cancellationToken).ConfigureAwait(false);
        var det = await _repo.GetDetailAsync(id, cancellationToken).ConfigureAwait(false);
        return det ?? throw new InvalidOperationException("No se pudo cargar la propuesta creada.");
    }

    public async Task<PropuestaDetailDto> ActualizarAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, UpdatePropuestaRequest request, CancellationToken cancellationToken = default)
    {
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        await EnsurePuedeEditarDocenteAsync(usuarioId, roles, det, cancellationToken).ConfigureAwait(false);
        if (det.EstadoActual != PropuestaEstados.Borrador)
            throw new InvalidOperationException("Solo se puede editar en estado borrador.");
        await _repo.UpdatePropuestaBasicaAsync(propuestaId, request, cancellationToken).ConfigureAwait(false);
        return await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
    }

    public async Task<PropuestaDetailDto> ObtenerAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, CancellationToken cancellationToken = default)
    {
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        await EnsurePuedeVerAsync(usuarioId, roles, det, cancellationToken).ConfigureAwait(false);
        return det;
    }

    public async Task<IReadOnlyList<PropuestaListItemDto>> ListarAsync(long usuarioId, IReadOnlyList<string> roles, string? estado, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = Math.Max(0, (page - 1) * pageSize);
        var take = Math.Clamp(pageSize, 1, 100);
        if (EsCpgic(roles))
            return await _repo.ListAsync(null, true, estado, skip, take, cancellationToken).ConfigureAwait(false);
        if (!EsDocente(roles))
            throw new UnauthorizedAccessException();
        var docenteId = await _repo.GetDocenteIdPorUsuarioAsync(usuarioId, cancellationToken).ConfigureAwait(false);
        if (!docenteId.HasValue)
            return Array.Empty<PropuestaListItemDto>();
        return await _repo.ListAsync(docenteId, false, estado, skip, take, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnviarRevisionAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, CancellationToken cancellationToken = default)
    {
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        await EnsurePuedeEditarDocenteAsync(usuarioId, roles, det, cancellationToken).ConfigureAwait(false);
        if (det.EstadoActual != PropuestaEstados.Borrador)
            throw new InvalidOperationException("Solo se puede enviar desde borrador.");
        await _repo.SetEstadoAsync(propuestaId, PropuestaEstados.EnRevision, DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false);
        await _repo.AddHistorialAsync(propuestaId, PropuestaEstados.Borrador, PropuestaEstados.EnRevision, usuarioId, "Enviado a revisión", cancellationToken).ConfigureAwait(false);
    }

    public async Task AprobarAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, TransicionRequest? request, CancellationToken cancellationToken = default)
    {
        if (!EsCpgic(roles))
            throw new UnauthorizedAccessException();
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        if (det.EstadoActual != PropuestaEstados.EnRevision)
            throw new InvalidOperationException("La propuesta no está en revisión.");
        ValidarCamposParaAprobar(det);
        await _repo.SetEstadoAsync(propuestaId, PropuestaEstados.Aprobada, null, cancellationToken).ConfigureAwait(false);
        await _repo.AddHistorialAsync(propuestaId, det.EstadoActual, PropuestaEstados.Aprobada, usuarioId, request?.Comentario, cancellationToken).ConfigureAwait(false);
    }

    public async Task RechazarAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, TransicionRequest? request, CancellationToken cancellationToken = default)
    {
        if (!EsCpgic(roles))
            throw new UnauthorizedAccessException();
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        if (det.EstadoActual != PropuestaEstados.EnRevision)
            throw new InvalidOperationException("La propuesta no está en revisión.");
        await _repo.SetEstadoAsync(propuestaId, PropuestaEstados.Rechazada, null, cancellationToken).ConfigureAwait(false);
        await _repo.AddHistorialAsync(propuestaId, det.EstadoActual, PropuestaEstados.Rechazada, usuarioId, request?.Comentario, cancellationToken).ConfigureAwait(false);
    }

    public async Task MarcarPendienteAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, TransicionRequest? request, CancellationToken cancellationToken = default)
    {
        if (!EsCpgic(roles))
            throw new UnauthorizedAccessException();
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        if (det.EstadoActual != PropuestaEstados.EnRevision)
            throw new InvalidOperationException("La propuesta no está en revisión.");
        await _repo.SetEstadoAsync(propuestaId, PropuestaEstados.Pendiente, null, cancellationToken).ConfigureAwait(false);
        await _repo.AddHistorialAsync(propuestaId, det.EstadoActual, PropuestaEstados.Pendiente, usuarioId, request?.Comentario, cancellationToken).ConfigureAwait(false);
    }

    public async Task AsignarEstudiantesAsync(long usuarioId, IReadOnlyList<string> roles, long propuestaId, AsignarEstudiantesRequest request, CancellationToken cancellationToken = default)
    {
        if (!EsDocente(roles))
            throw new UnauthorizedAccessException();
        var det = await _repo.GetDetailAsync(propuestaId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Propuesta no encontrada.");
        await EnsurePuedeEditarDocenteAsync(usuarioId, roles, det, cancellationToken).ConfigureAwait(false);
        if (det.EstadoActual != PropuestaEstados.Aprobada)
            throw new InvalidOperationException("Solo propuestas aprobadas permiten asignar estudiantes.");
        if (!await _repo.EstudiantesExistenAsync(request.EstudianteIds, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("Uno o más estudiantes no existen.");
        await _repo.LimpiarAsignacionesAsync(propuestaId, cancellationToken).ConfigureAwait(false);
        await _repo.AsignarEstudiantesAsync(propuestaId, request.EstudianteIds, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidarCamposParaAprobar(PropuestaDetailDto det)
    {
        if (string.IsNullOrWhiteSpace(det.Titulo)
            || string.IsNullOrWhiteSpace(det.Descripcion)
            || string.IsNullOrWhiteSpace(det.Problema)
            || string.IsNullOrWhiteSpace(det.ObjetivoGeneral)
            || string.IsNullOrWhiteSpace(det.Alcance))
            throw new InvalidOperationException("No se puede aprobar: complete descripción, problema, objetivo y alcance.");
    }

    private async Task EnsurePuedeVerAsync(long usuarioId, IReadOnlyList<string> roles, PropuestaDetailDto det, CancellationToken cancellationToken)
    {
        if (EsCpgic(roles))
            return;
        var docenteId = await _repo.GetDocenteIdPorUsuarioAsync(usuarioId, cancellationToken).ConfigureAwait(false);
        if (docenteId.HasValue && det.DocenteId == docenteId.Value)
            return;
        throw new UnauthorizedAccessException();
    }

    private async Task EnsurePuedeEditarDocenteAsync(long usuarioId, IReadOnlyList<string> roles, PropuestaDetailDto det, CancellationToken cancellationToken)
    {
        if (EsAdmin(roles))
            return;
        if (!EsDocente(roles))
            throw new UnauthorizedAccessException();
        var docenteId = await _repo.GetDocenteIdPorUsuarioAsync(usuarioId, cancellationToken).ConfigureAwait(false);
        if (!docenteId.HasValue || det.DocenteId != docenteId.Value)
            throw new UnauthorizedAccessException();
    }
}

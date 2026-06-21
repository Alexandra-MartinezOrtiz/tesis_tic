using Microsoft.EntityFrameworkCore;
using Propuestas.Application.Abstractions;
using Propuestas.Application.Dtos;
using Propuestas.Application.Domain;
using Propuestas.Infrastructure.Persistence.Scaffolded;

namespace Propuestas.Infrastructure.Repositories;

public class PropuestaRepository : IPropuestaRepository
{
    private readonly TicfisPropuestasDbContext _db;

    public PropuestaRepository(TicfisPropuestasDbContext db)
    {
        _db = db;
    }

    public async Task<long> EnsureDocenteAsync(long usuarioIdReferencia, string? email = null, CancellationToken cancellationToken = default)
    {
        var existente = await _db.Docentes
            .FirstOrDefaultAsync(d => d.UsuarioIdReferencia == usuarioIdReferencia, cancellationToken)
            .ConfigureAwait(false);
        if (existente is not null)
        {
            if (email is not null && existente.Email != email)
            {
                existente.Email = email;
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return existente.Id;
        }
        var d = new Docente { UsuarioIdReferencia = usuarioIdReferencia, Email = email };
        _db.Docentes.Add(d);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return d.Id;
    }

    public Task<bool> CodigoExisteAsync(string codigo, long? excludePropuestaId, CancellationToken cancellationToken = default)
    {
        var q = _db.Propuestas.AsNoTracking().Where(p => p.Codigo == codigo);
        if (excludePropuestaId.HasValue)
            q = q.Where(p => p.Id != excludePropuestaId.Value);
        return q.AnyAsync(cancellationToken);
    }

    public async Task<long> InsertPropuestaAsync(CreatePropuestaRequest request, long docenteId, long actorUsuarioId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var p = new Propuesta
        {
            Codigo = request.Codigo,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            Problema = request.Problema,
            ObjetivoGeneral = request.ObjetivoGeneral,
            Alcance = request.Alcance,
            DocenteId = docenteId,
            // Alcance del módulo Consultas y Reportes: las propuestas creadas quedan
            // directamente aprobadas (visibles en Reportes), no en borrador.
            EstadoActual = PropuestaEstados.Aprobada,
            FechaEnvio = null,
            FechaUltimaActualizacion = now,
            Activa = true,
            EstudiantesPropuestos = Math.Clamp(request.EstudiantesPropuestos, 0, 5),
            Carrera = request.Carrera,
            Asignaturas = request.Asignaturas,
            AutorizadoPor = request.AutorizadoPor,
            FechaAutorizacion = request.FechaAutorizacion,
            PresentadoPor = request.PresentadoPor,
            EstudiantesNombres = request.EstudiantesNombres,
            ResolucionCpgic = request.ResolucionCpgic,
            PresidenteCpgic = request.PresidenteCpgic,
            FechaAprobacion = request.FechaAprobacion,
        };
        _db.Propuestas.Add(p);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await AddHistorialAsync(p.Id, null, PropuestaEstados.Aprobada, actorUsuarioId, "Creación (aprobada)", cancellationToken).ConfigureAwait(false);
        return p.Id;
    }

    public async Task<PropuestaDetailDto?> GetDetailAsync(long propuestaId, CancellationToken cancellationToken = default)
    {
        var p = await _db.Propuestas
            .AsNoTracking()
            .Include(x => x.Docente)
            .Include(x => x.PropuestaEstudiantes)
            .ThenInclude(x => x.Estudiante)
            .Include(x => x.PropuestaObservaciones)
            .FirstOrDefaultAsync(x => x.Id == propuestaId, cancellationToken)
            .ConfigureAwait(false);
        if (p is null)
            return null;
        var est = p.PropuestaEstudiantes.Select(pe => new EstudianteAsignadoDto(
            pe.EstudianteId,
            $"{pe.Estudiante.Nombres} {pe.Estudiante.Apellidos}".Trim(),
            pe.Estudiante.Email,
            pe.FechaAsignacion)).ToList();
        var obs = p.PropuestaObservaciones
            .OrderByDescending(o => o.CreadoEn)
            .Select(o => new ObservacionDto(o.Id, o.Observacion, o.CreadoPorUsuarioId, o.CreadoEn))
            .ToList();
        return new PropuestaDetailDto(
            p.Id,
            p.Codigo,
            p.Titulo,
            p.Descripcion,
            p.Problema,
            p.ObjetivoGeneral,
            p.Alcance,
            p.DocenteId,
            p.Docente.UsuarioIdReferencia,
            p.EstadoActual,
            p.FechaEnvio,
            p.FechaUltimaActualizacion,
            p.Activa,
            p.EstudiantesPropuestos,
            est,
            obs,
            p.Carrera,
            p.Asignaturas,
            p.AutorizadoPor,
            p.FechaAutorizacion,
            p.PresentadoPor,
            p.EstudiantesNombres,
            p.ResolucionCpgic,
            p.PresidenteCpgic,
            p.FechaAprobacion);
    }

    public async Task UpdatePropuestaBasicaAsync(long propuestaId, UpdatePropuestaRequest request, CancellationToken cancellationToken = default)
    {
        var p = await _db.Propuestas.FirstOrDefaultAsync(x => x.Id == propuestaId, cancellationToken).ConfigureAwait(false);
        if (p is null)
            throw new InvalidOperationException("Propuesta no encontrada.");
        p.Titulo = request.Titulo;
        p.Descripcion = request.Descripcion;
        p.Problema = request.Problema;
        p.ObjetivoGeneral = request.ObjetivoGeneral;
        p.Alcance = request.Alcance;
        p.EstudiantesPropuestos = Math.Clamp(request.EstudiantesPropuestos, 0, 5);
        p.Carrera = request.Carrera;
        p.Asignaturas = request.Asignaturas;
        p.AutorizadoPor = request.AutorizadoPor;
        p.FechaAutorizacion = request.FechaAutorizacion;
        p.PresentadoPor = request.PresentadoPor;
        p.EstudiantesNombres = request.EstudiantesNombres;
        p.ResolucionCpgic = request.ResolucionCpgic;
        p.PresidenteCpgic = request.PresidenteCpgic;
        p.FechaAprobacion = request.FechaAprobacion;
        p.FechaUltimaActualizacion = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SetEstadoAsync(long propuestaId, string estadoNuevo, DateTimeOffset? fechaEnvio, CancellationToken cancellationToken = default)
    {
        var p = await _db.Propuestas.FirstOrDefaultAsync(x => x.Id == propuestaId, cancellationToken).ConfigureAwait(false);
        if (p is null)
            throw new InvalidOperationException("Propuesta no encontrada.");
        p.EstadoActual = estadoNuevo;
        p.FechaUltimaActualizacion = DateTimeOffset.UtcNow;
        if (fechaEnvio.HasValue)
            p.FechaEnvio = fechaEnvio;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddHistorialAsync(long propuestaId, string? estadoAnterior, string estadoNuevo, long cambiadoPorUsuarioId, string? comentario, CancellationToken cancellationToken = default)
    {
        _db.PropuestaHistorialEstados.Add(new PropuestaHistorialEstado
        {
            PropuestaId = propuestaId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            CambiadoPorUsuarioId = cambiadoPorUsuarioId,
            Comentario = comentario,
            CambiadoEn = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PropuestaListItemDto>> ListAsync(long? docenteId, bool verTodas, string? estado, int skip, int take, CancellationToken cancellationToken = default)
    {
        var q = _db.Propuestas.AsNoTracking().AsQueryable();
        if (!verTodas)
        {
            if (!docenteId.HasValue)
                return Array.Empty<PropuestaListItemDto>();
            q = q.Where(p => p.DocenteId == docenteId.Value);
        }
        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.EstadoActual == estado);
        var list = await q
            .OrderByDescending(p => p.FechaUltimaActualizacion)
            .Skip(skip)
            .Take(take)
            .Select(p => new PropuestaListItemDto(p.Id, p.Codigo, p.Titulo, p.EstadoActual, p.FechaUltimaActualizacion, p.Activa, p.Docente.Email, p.EstudiantesPropuestos))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return list;
    }

    public async Task<long?> GetDocenteIdPorUsuarioAsync(long usuarioIdReferencia, CancellationToken cancellationToken = default)
    {
        return await _db.Docentes.AsNoTracking()
            .Where(d => d.UsuarioIdReferencia == usuarioIdReferencia)
            .Select(d => (long?)d.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> PropuestaPerteneceADocenteAsync(long propuestaId, long docenteId, CancellationToken cancellationToken = default)
    {
        return _db.Propuestas.AsNoTracking()
            .AnyAsync(p => p.Id == propuestaId && p.DocenteId == docenteId, cancellationToken);
    }

    public async Task<bool> EstudiantesExistenAsync(IReadOnlyList<long> estudianteIds, CancellationToken cancellationToken = default)
    {
        if (estudianteIds.Count == 0)
            return true;
        var distinct = estudianteIds.Distinct().ToList();
        var count = await _db.Estudiantes.AsNoTracking()
            .CountAsync(e => distinct.Contains(e.Id), cancellationToken)
            .ConfigureAwait(false);
        return count == distinct.Count;
    }

    public async Task LimpiarAsignacionesAsync(long propuestaId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.PropuestaEstudiantes.Where(x => x.PropuestaId == propuestaId).ToListAsync(cancellationToken).ConfigureAwait(false);
        _db.PropuestaEstudiantes.RemoveRange(rows);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AsignarEstudiantesAsync(long propuestaId, IReadOnlyList<long> estudianteIds, CancellationToken cancellationToken = default)
    {
        foreach (var eid in estudianteIds.Distinct())
        {
            _db.PropuestaEstudiantes.Add(new PropuestaEstudiante
            {
                PropuestaId = propuestaId,
                EstudianteId = eid,
                FechaAsignacion = DateTimeOffset.UtcNow,
            });
        }
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

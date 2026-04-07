using Identity.Application.Abstractions;
using Identity.Application.Dtos;
using Identity.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly TicfisIdentityDbContext _db;

    public UsuarioRepository(TicfisIdentityDbContext db)
    {
        _db = db;
    }

    public async Task<UsuarioDetalleDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        return u is null ? null : MapDetalle(u);
    }

    public async Task<UsuarioDetalleDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
            .ConfigureAwait(false);
        return u is null ? null : MapDetalle(u);
    }

    public async Task<IReadOnlyList<UsuarioListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles)
            .ThenInclude(x => x.Rol)
            .OrderBy(x => x.Apellidos)
            .ThenBy(x => x.Nombres)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return list.Select(x => new UsuarioListItemDto(
            x.Id,
            x.Nombres,
            x.Apellidos,
            x.Email,
            x.Activo,
            x.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList())).ToList();
    }

    public async Task<long> CreateAsync(string nombres, string apellidos, string email, string passwordHash, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var u = new Usuario
        {
            Nombres = nombres,
            Apellidos = apellidos,
            Email = email,
            PasswordHash = passwordHash,
            Activo = true,
            CreadoEn = now,
            ActualizadoEn = now,
        };
        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return u.Id;
    }

    public async Task UpdateAsync(long id, string nombres, string apellidos, string email, bool activo, CancellationToken cancellationToken = default)
    {
        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (u is null)
            throw new InvalidOperationException("Usuario no encontrado.");
        u.Nombres = nombres;
        u.Apellidos = apellidos;
        u.Email = email;
        u.Activo = activo;
        u.ActualizadoEn = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> EmailExistsAsync(string email, long? excludeId, CancellationToken cancellationToken = default)
    {
        var q = _db.Usuarios.AsNoTracking().Where(x => x.Email == email);
        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);
        return await q.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SetRolesAsync(long usuarioId, IReadOnlyList<long> rolIds, CancellationToken cancellationToken = default)
    {
        var existing = await _db.UsuarioRoles.Where(x => x.UsuarioId == usuarioId).ToListAsync(cancellationToken).ConfigureAwait(false);
        _db.UsuarioRoles.RemoveRange(existing);
        foreach (var rid in rolIds.Distinct())
            _db.UsuarioRoles.Add(new UsuarioRole { UsuarioId = usuarioId, RolId = rid });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RolDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _db.Roles.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken).ConfigureAwait(false);
        return roles.Select(r => new RolDto(r.Id, r.Nombre, r.Descripcion)).ToList();
    }

    public async Task StoreRefreshTokenAsync(long usuarioId, Guid jti, DateTimeOffset expiraEn, CancellationToken cancellationToken = default)
    {
        _db.TokensRevocados.Add(new TokensRevocado
        {
            UsuarioId = usuarioId,
            TokenJti = jti,
            ExpiraEn = expiraEn,
            RevocadoEn = null,
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> RefreshTokenIsActiveAsync(Guid jti, CancellationToken cancellationToken = default)
    {
        var t = await _db.TokensRevocados
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenJti == jti, cancellationToken)
            .ConfigureAwait(false);
        if (t is null)
            return false;
        if (t.RevocadoEn.HasValue)
            return false;
        return t.ExpiraEn > DateTimeOffset.UtcNow;
    }

    public async Task RevokeRefreshTokenAsync(Guid jti, CancellationToken cancellationToken = default)
    {
        var t = await _db.TokensRevocados.FirstOrDefaultAsync(x => x.TokenJti == jti, cancellationToken).ConfigureAwait(false);
        if (t is null)
            return;
        t.RevocadoEn = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<string?> GetPasswordHashByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _db.Usuarios
            .AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => x.PasswordHash)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static UsuarioDetalleDto MapDetalle(Usuario u)
    {
        var roles = u.UsuarioRoles
            .Select(ur => new RolDto(ur.Rol.Id, ur.Rol.Nombre, ur.Rol.Descripcion))
            .ToList();
        return new UsuarioDetalleDto(u.Id, u.Nombres, u.Apellidos, u.Email, u.Activo, roles);
    }
}

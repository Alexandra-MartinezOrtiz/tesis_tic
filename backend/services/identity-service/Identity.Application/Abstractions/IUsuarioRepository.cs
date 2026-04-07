using Identity.Application.Dtos;

namespace Identity.Application.Abstractions;

public interface IUsuarioRepository
{
    Task<UsuarioDetalleDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UsuarioDetalleDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioListItemDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<long> CreateAsync(string nombres, string apellidos, string email, string passwordHash, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, string nombres, string apellidos, string email, bool activo, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, long? excludeId, CancellationToken cancellationToken = default);
    Task SetRolesAsync(long usuarioId, IReadOnlyList<long> rolIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task StoreRefreshTokenAsync(long usuarioId, Guid jti, DateTimeOffset expiraEn, CancellationToken cancellationToken = default);
    Task<bool> RefreshTokenIsActiveAsync(Guid jti, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(Guid jti, CancellationToken cancellationToken = default);
    Task<string?> GetPasswordHashByEmailAsync(string email, CancellationToken cancellationToken = default);
}

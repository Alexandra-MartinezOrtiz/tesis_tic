using Identity.Application.Dtos;

namespace Identity.Application.Abstractions;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioListItemDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDetalleDto> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<UsuarioDetalleDto> UpdateAsync(long id, UpdateUsuarioRequest request, CancellationToken cancellationToken = default);
    Task AsignarRolesAsync(long id, AsignarRolesRequest request, CancellationToken cancellationToken = default);
}

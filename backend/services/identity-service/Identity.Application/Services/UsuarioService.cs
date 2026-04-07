using Identity.Application.Abstractions;
using Identity.Application.Dtos;

namespace Identity.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(IUsuarioRepository usuarios, IPasswordHasher passwordHasher)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
    }

    public Task<IReadOnlyList<UsuarioListItemDto>> ListAsync(CancellationToken cancellationToken = default)
        => _usuarios.ListAsync(cancellationToken);

    public async Task<UsuarioDetalleDto> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        if (await _usuarios.EmailExistsAsync(request.Email, null, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("El correo ya está registrado.");

        var hash = _passwordHasher.Hash(request.Password);
        var id = await _usuarios.CreateAsync(request.Nombres, request.Apellidos, request.Email, hash, cancellationToken)
            .ConfigureAwait(false);
        if (request.RolIds is { Count: > 0 })
            await _usuarios.SetRolesAsync(id, request.RolIds, cancellationToken).ConfigureAwait(false);

        var created = await _usuarios.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return created ?? throw new InvalidOperationException("No se pudo crear el usuario.");
    }

    public async Task<UsuarioDetalleDto> UpdateAsync(long id, UpdateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        if (await _usuarios.EmailExistsAsync(request.Email, id, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("El correo ya está en uso.");

        await _usuarios.UpdateAsync(id, request.Nombres, request.Apellidos, request.Email, request.Activo, cancellationToken)
            .ConfigureAwait(false);
        var updated = await _usuarios.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return updated ?? throw new InvalidOperationException("Usuario no encontrado.");
    }

    public Task AsignarRolesAsync(long id, AsignarRolesRequest request, CancellationToken cancellationToken = default)
        => _usuarios.SetRolesAsync(id, request.RolIds, cancellationToken);
}

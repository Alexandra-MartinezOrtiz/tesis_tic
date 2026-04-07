namespace Identity.Application.Dtos;

public record RolDto(long Id, string Nombre, string? Descripcion);

public record UsuarioListItemDto(long Id, string Nombres, string Apellidos, string Email, bool Activo, IReadOnlyList<string> Roles);

public record UsuarioDetalleDto(long Id, string Nombres, string Apellidos, string Email, bool Activo, IReadOnlyList<RolDto> Roles);

public record CreateUsuarioRequest(string Nombres, string Apellidos, string Email, string Password, IReadOnlyList<long>? RolIds);

public record UpdateUsuarioRequest(string Nombres, string Apellidos, string Email, bool Activo);

public record AsignarRolesRequest(IReadOnlyList<long> RolIds);

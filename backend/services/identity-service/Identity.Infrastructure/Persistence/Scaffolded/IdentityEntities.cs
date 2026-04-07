namespace Identity.Infrastructure.Persistence.Scaffolded;

public class Usuario
{
    public long Id { get; set; }
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool Activo { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public DateTimeOffset ActualizadoEn { get; set; }
    public ICollection<UsuarioRole> UsuarioRoles { get; set; } = new List<UsuarioRole>();
    public ICollection<TokensRevocado> TokensRevocados { get; set; } = new List<TokensRevocado>();
}

public class Rol
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public ICollection<UsuarioRole> UsuarioRoles { get; set; } = new List<UsuarioRole>();
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}

public class Permiso
{
    public long Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}

public class UsuarioRole
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public long RolId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}

public class RolPermiso
{
    public long Id { get; set; }
    public long RolId { get; set; }
    public long PermisoId { get; set; }
    public Rol Rol { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}

public class TokensRevocado
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public Guid TokenJti { get; set; }
    public DateTimeOffset ExpiraEn { get; set; }
    public DateTimeOffset? RevocadoEn { get; set; }
    public Usuario Usuario { get; set; } = null!;
}

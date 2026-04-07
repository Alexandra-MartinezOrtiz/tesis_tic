using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(TicfisIdentityDbContext db, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        if (await db.Roles.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var roles = new[]
        {
            new Rol { Nombre = "Docente", Descripcion = "Docente proponente" },
            new Rol { Nombre = "CPGIC", Descripcion = "Comité de propuestas" },
            new Rol { Nombre = "Admin", Descripcion = "Administración" },
        };
        db.Roles.AddRange(roles);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var admin = new Usuario
        {
            Nombres = "Admin",
            Apellidos = "Sistema",
            Email = "admin@ticfis.local",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            ActualizadoEn = DateTimeOffset.UtcNow,
        };
        db.Usuarios.Add(admin);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var adminRol = await db.Roles.AsNoTracking().FirstAsync(x => x.Nombre == "Admin", cancellationToken).ConfigureAwait(false);
        db.UsuarioRoles.Add(new UsuarioRole { UsuarioId = admin.Id, RolId = adminRol.Id });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

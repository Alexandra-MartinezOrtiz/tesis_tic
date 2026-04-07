using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propuestas.Infrastructure.Persistence.Scaffolded;

namespace Propuestas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPropuestasInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<TicfisPropuestasDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        return services;
    }
}

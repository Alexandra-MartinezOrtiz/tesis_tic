using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Propuestas.Application.Abstractions;
using Propuestas.Application.Services;
using Propuestas.Infrastructure.Persistence.Scaffolded;
using Propuestas.Infrastructure.Repositories;

namespace Propuestas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPropuestasInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<TicfisPropuestasDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.AddScoped<IPropuestaRepository, PropuestaRepository>();
        services.AddScoped<IPropuestaService, PropuestaService>();
        return services;
    }
}

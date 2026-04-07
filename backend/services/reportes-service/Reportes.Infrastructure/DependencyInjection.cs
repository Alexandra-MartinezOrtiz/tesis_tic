using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reportes.Application.Abstractions;
using Reportes.Infrastructure.Persistence.Scaffolded;
using Reportes.Infrastructure.Services;

namespace Reportes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<TicfisReportesDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.AddHttpClient();
        services.AddScoped<IReportesService, ReportesConsultaService>();
        return services;
    }
}

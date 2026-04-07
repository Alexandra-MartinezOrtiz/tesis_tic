using Reportes.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReportesInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "reportes" }));

app.Run();

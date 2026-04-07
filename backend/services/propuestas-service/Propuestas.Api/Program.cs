using Propuestas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPropuestasInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "propuestas" }));

app.Run();

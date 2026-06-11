using System.Text;
using Identity.Application.Abstractions;
using Identity.Application.Options;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Scaffolded;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT (sin escribir 'Bearer')",
    });
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la sección Jwt en configuración.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapGet("/swagger-auth.js", () => Results.Text("""
        (function () {
            var _fetch = window.fetch;
            window.fetch = function (input, init) {
                init = init || {};
                init.headers = init.headers || {};
                try {
                    var raw = localStorage.getItem('authorized');
                    if (raw) {
                        var auth = JSON.parse(raw);
                        if (auth.Bearer && auth.Bearer.value) {
                            init.headers['Authorization'] = 'Bearer ' + auth.Bearer.value;
                        }
                    }
                } catch (e) {}
                return _fetch(input, init);
            };
        })();
        """, "application/javascript"));
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
        c.InjectJavascript("/swagger-auth.js");
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicfisIdentityDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await IdentityDataSeeder.SeedAsync(db, hasher).ConfigureAwait(false);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "identity" }));

app.Run();

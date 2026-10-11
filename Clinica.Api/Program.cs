using System.Text;
using Clinica.Api.Documentacion;
using Clinica.Api.Middleware;
using Clinica.Api.Seguridad;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Identidad.Puertos;
using Clinica.Aplicacion.Identidad.Servicios;
using Clinica.Dominio.Identidad;
using Clinica.Infraestructura.Identidad.Repositorios;
using Clinica.Infraestructura.Identidad.Seguridad;
using Clinica.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de datos con MySQL Pomelo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClinicaDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 2. Controladores y OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer<BearerSecurityTransformer>());

// 3. Errores -> respuestas HTTP
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

// 4. CORS (orígenes leídos desde la configuración)
builder.Services.AddCors(opciones => opciones.AddPolicy("Frontend", politica => politica
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
    .AllowAnyHeader()
    .AllowAnyMethod()));

// 5. Configuración JWT con validación al inicio
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.Seccion))
    .Validate(s => s.SecretKey.Length >= 32, "Jwt:SecretKey debe tener al menos 32 caracteres.")
    .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience),
        "Jwt:Issuer y Jwt:Audience son obligatorios.")
    .Validate(s => s.ExpirationMinutes > 0, "Jwt:ExpirationMinutes debe ser mayor a 0.")
    .ValidateOnStart();

// 6. Autenticación JWT Bearer
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((opciones, jwt) =>
    {
        var s = jwt.Value;

        // No renombrar claims: "sub" sigue siendo "sub" y "role" sigue siendo "role"
        opciones.MapInboundClaims = false;

        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = s.Issuer,
            ValidateAudience = true,
            ValidAudience = s.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimNombres.Rol,        // para [Authorize(Roles = "...")]
            NameClaimType = ClaimNombres.UsuarioId
        };
    });

builder.Services.AddAuthorization();

// 7. Inyección de dependencias de Identidad
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 8. Usuario actual / Contexto HTTP
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();

var app = builder.Build();

// 9. Migraciones automáticas + Seeder (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ClinicaDbContext>();
    await db.Database.MigrateAsync();

    var passwordSeed = app.Configuration["Seed:Password"];
    if (string.IsNullOrWhiteSpace(passwordSeed))
    {
        app.Logger.LogWarning("Seed:Password no está configurada: no se cargan usuarios de prueba.");
    }
    else
    {
        await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), passwordSeed);
    }
}

// 10. Pipeline HTTP y Middlewares
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// CORS antes de Authentication y Authorization
app.UseCors("Frontend");

// El orden importa: primero "quién sos", después "qué podés hacer"
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using Clinica.Dominio;
using Clinica.Dominio.Pacientes;
using Clinica.Dominio.Turnos;
using Clinica.Infraestructura;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configurar SQLite
builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlite("Data Source=clinica.db"));

// 3. Configurar CORS para permitir que el Frontend (React/Vite) se conecte
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 4. Inicializar y sembrar la base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ClinicaDbContext>();
    await DbSeeder.InicializarAsync(context);
}

// 5. Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
//app.UseHttpsRedirection();

// ==========================================
// ENDPOINTS DE LA API (MINIMAL APIs)
// ==========================================

// GET: Listar odontólogos (para selectores de login y de agenda)
app.MapGet("/api/odontologos", async (ClinicaDbContext db) =>
{
    var odontologos = await db.Usuarios
        .Where(u => u.Rol == Rol.Odontologo && u.Activo)
        .Select(u => new { u.Id, u.Nombre, u.Apellido, u.Email })
        .ToListAsync();

    return Results.Ok(odontologos);
});

// GET: Listar usuarios completos (para el selector de login rápido)
app.MapGet("/api/usuarios", async (ClinicaDbContext db) =>
{
    var usuarios = await db.Usuarios
        .Where(u => u.Activo)
        .Select(u => new { u.Id, u.Nombre, u.Apellido, Rol = u.Rol.ToString() })
        .ToListAsync();

    return Results.Ok(usuarios);
});

// GET: Obtener turnos de un odontólogo en una fecha (separados por tipo)
app.MapGet("/api/turnos", async (int odontologoId, string fecha, ClinicaDbContext db) =>
{
    if (!DateOnly.TryParse(fecha, out var fechaParsed))
        return Results.BadRequest("Formato de fecha inválido. Usar yyyy-MM-dd");

    var turnos = await db.Turnos
        .Where(t => t.OdontologoId == odontologoId && t.Fecha == fechaParsed)
        .OrderBy(t => t.HoraInicio)
        .ThenBy(t => t.OrdenEntreTurno)
        .ToListAsync();

    var programados = turnos.Where(t => t.Tipo == TipoTurno.Programado).ToList();
    var entreTurnos = turnos.Where(t => t.Tipo == TipoTurno.EntreTurno).ToList();

    return Results.Ok(new
    {
        Fecha = fechaParsed.ToString("yyyy-MM-dd"),
        OdontologoId = odontologoId,
        Programados = programados,
        EntreTurnos = entreTurnos
    });
});

// POST: Crear nuevo turno (Programado o EntreTurno)
app.MapPost("/api/turnos", async (CrearTurnoDto dto, ClinicaDbContext db) =>
{
    if (!DateOnly.TryParse(dto.Fecha, out var fechaParsed))
        return Results.BadRequest("Fecha inválida.");

    Turno nuevoTurno;

    if (dto.EsEntreTurno)
    {
        // Calcular el siguiente orden de entre-turno para este odontólogo en este día
        var ultimoOrden = await db.Turnos
            .Where(t => t.OdontologoId == dto.OdontologoId &&
                        t.Fecha == fechaParsed &&
                        t.Tipo == TipoTurno.EntreTurno)
            .MaxAsync(t => (int?)t.OrdenEntreTurno) ?? 0;

        nuevoTurno = Turno.CrearEntreTurno(
            pacienteNombre: dto.PacienteNombre,
            pacienteTelefono: dto.PacienteTelefono,
            odontologoId: dto.OdontologoId,
            fecha: fechaParsed,
            ordenCalculado: ultimoOrden + 1,
            origen: OrigenCreacion.PresencialSecretaria,
            creadoPor: dto.CreadoPor,
            notas: dto.Notas
        );
    }
    else
    {
        if (!TimeOnly.TryParse(dto.HoraInicio, out var horaIni) ||
            !TimeOnly.TryParse(dto.HoraFin, out var horaFin))
        {
            return Results.BadRequest("Horas de inicio o fin inválidas (usar HH:mm).");
        }

        nuevoTurno = Turno.CrearProgramado(
            pacienteNombre: dto.PacienteNombre,
            pacienteTelefono: dto.PacienteTelefono,
            odontologoId: dto.OdontologoId,
            fecha: fechaParsed,
            horaInicio: horaIni,
            horaFin: horaFin,
            origen: OrigenCreacion.PresencialSecretaria,
            creadoPor: dto.CreadoPor,
            notas: dto.Notas
        );
    }

    db.Turnos.Add(nuevoTurno);
    await db.SaveChangesAsync();

    return Results.Created($"/api/turnos/{nuevoTurno.Id}", nuevoTurno);
});

// PATCH: Iniciar atención de un turno
app.MapPatch("/api/turnos/{id:int}/iniciar", async (int id, ClinicaDbContext db) =>
{
    var turno = await db.Turnos.FindAsync(id);
    if (turno is null)
        return Results.NotFound("Turno no encontrado.");

    try
    {
        turno.IniciarAtencion();
        await db.SaveChangesAsync();
        return Results.Ok(turno);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// PATCH: Finalizar atención de un turno
app.MapPatch("/api/turnos/{id:int}/finalizar", async (int id, ClinicaDbContext db) =>
{
    var turno = await db.Turnos.FindAsync(id);
    if (turno is null)
        return Results.NotFound("Turno no encontrado.");

    try
    {
        turno.FinalizarAtencion();
        await db.SaveChangesAsync();
        return Results.Ok(turno);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();

// DTO para recepción de datos
public record CrearTurnoDto(
    string PacienteNombre,
    string PacienteTelefono,
    int OdontologoId,
    string Fecha,
    string? HoraInicio,
    string? HoraFin,
    bool EsEntreTurno,
    int CreadoPor,
    string? Notas
);
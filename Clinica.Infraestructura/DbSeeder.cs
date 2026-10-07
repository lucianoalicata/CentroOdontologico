using Clinica.Dominio;
using Clinica.Dominio.Pacientes;
using Clinica.Dominio.Turnos;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura;

public static class DbSeeder
{
    public static async Task InicializarAsync(ClinicaDbContext context)
    {
        // Asegura que la BD SQLite y las tablas existan sin necesidad de correr migraciones manuales
        await context.Database.EnsureCreatedAsync();

        if (await context.Usuarios.AnyAsync())
            return; // Ya hay datos cargados

        var secretaria = new Usuario("Lucía", "Gómez", "secretaria@clinica.com", Rol.Secretaria);
        var odonto1 = new Usuario("Martín", "Pérez", "martin.perez@clinica.com", Rol.Odontologo);
        var odonto2 = new Usuario("Valeria", "Sánchez", "valeria.sanchez@clinica.com", Rol.Odontologo);

        context.Usuarios.AddRange(secretaria, odonto1, odonto2);
        await context.SaveChangesAsync();

        // Opcional: sembrar 1 turno de prueba para hoy
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var turnoEjemplo = Turno.CrearProgramado(
            pacienteNombre: "Carlos Benítez",
            pacienteTelefono: "3815551234",
            odontologoId: odonto1.Id,
            fecha: hoy,
            horaInicio: new TimeOnly(9, 0),
            horaFin: new TimeOnly(9, 30),
            origen: OrigenCreacion.TelefonoSecretaria,
            creadoPor: secretaria.Id,
            notas: "Primera consulta por dolor en molar"
        );

        context.Turnos.Add(turnoEjemplo);
        await context.SaveChangesAsync();
    }
}
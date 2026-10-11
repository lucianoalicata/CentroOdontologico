using Clinica.Aplicacion.Identidad.Puertos;
using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Persistencia;

// Datos de prueba SOLO para desarrollo. Es idempotente: si ya hay usuarios, no hace nada.
public static class DbSeeder
{
    public static async Task SeedAsync(
        ClinicaDbContext db,
        IPasswordHasher hasher,
        string password,
        CancellationToken ct = default)
    {
        // IgnoreQueryFilters: cuenta también a los usuarios dados de baja
        if (await db.Set<Usuario>().IgnoreQueryFilters().AnyAsync(ct))
            return;

        // Una transacción: o se carga todo o no se carga nada
        await using var transaccion = await db.Database.BeginTransactionAsync(ct);

        var hash = hasher.Hashear(password);

        var admin = new Usuario("Admin", "Sistema", "admin@clinica.test", hash, RolUsuario.Administrador);
        var secretaria = new Usuario("Sofía", "Gómez", "secretaria@clinica.test", hash, RolUsuario.Secretaria);
        var odontologo = new Usuario("Carolina", "Díaz", "odontologo@clinica.test", hash, RolUsuario.Odontologo);
        var paciente = new Usuario("Juan", "Pérez", "paciente@clinica.test", hash, RolUsuario.Paciente);

        db.Set<Usuario>().AddRange(admin, secretaria, odontologo, paciente);
        await db.SaveChangesAsync(ct);   // acá la base les asigna el Id

        // Los perfiles necesitan el Id del usuario, por eso se guardan en un segundo paso
        db.Set<SecretariaPerfil>().Add(new SecretariaPerfil(secretaria.Id, "S-001"));
        db.Set<OdontologoPerfil>().Add(new OdontologoPerfil(odontologo.Id, "MP-1001", "Odontología general"));
        db.Set<PacientePerfil>().AddRange(
            new PacientePerfil("Juan", "Pérez", "30111222", "1155550001", new DateOnly(1990, 5, 14), paciente.Id),
            // Paciente sin cuenta web: así lo registra la secretaria en el mostrador
            new PacientePerfil("María", "López", "12345678", "1155550002", new DateOnly(1948, 3, 2)));
        await db.SaveChangesAsync(ct);

        await transaccion.CommitAsync(ct);
    }
}
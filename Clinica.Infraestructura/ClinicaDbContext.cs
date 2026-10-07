using Clinica.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            builder.Property(u => u.Rol).IsRequired();
            builder.Property(u => u.Activo).HasDefaultValue(true);
        });

        // Configuración de Turno
        modelBuilder.Entity<Turno>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.PacienteWalkInNombre).IsRequired().HasMaxLength(150);
            builder.Property(t => t.PacienteWalkInTelefono).HasMaxLength(50);
            builder.Property(t => t.Estado).IsRequired();
            builder.Property(t => t.Tipo).IsRequired();
            builder.Property(t => t.Origen).IsRequired();
            builder.Property(t => t.Notas).HasMaxLength(500);

            // Mapeo seguro para tipos DateOnly y TimeOnly en SQLite
            builder.Property(t => t.Fecha)
                   .HasConversion(
                       d => d.ToString("yyyy-MM-dd"),
                       s => DateOnly.Parse(s));

            builder.Property(t => t.HoraInicio)
                   .HasConversion(
                       t => t.HasValue ? t.Value.ToString("HH:mm:ss") : null,
                       s => !string.IsNullOrEmpty(s) ? TimeOnly.Parse(s) : null);

            builder.Property(t => t.HoraFin)
                   .HasConversion(
                       t => t.HasValue ? t.Value.ToString("HH:mm:ss") : null,
                       s => !string.IsNullOrEmpty(s) ? TimeOnly.Parse(s) : null);
        });
    }
}
using Clinica.Dominio.Identidad;
using Clinica.Dominio.Turnos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.Turnos;

public class TurnoConfig : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Tipo).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Estado).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Origen).IsRequired().HasConversion<string>().HasMaxLength(30);

        builder.Property(t => t.PacienteWalkInNombre).HasMaxLength(Turno.LongitudMaximaNombre);
        builder.Property(t => t.PacienteWalkInTelefono).HasMaxLength(Turno.LongitudMaximaTelefono);
        builder.Property(t => t.PacienteWalkInDni).HasMaxLength(Turno.LongitudMaximaDni);
        builder.Property(t => t.Motivo).HasMaxLength(Turno.LongitudMaximaMotivo);
        builder.Property(t => t.MotivoCancelacion).HasMaxLength(Turno.LongitudMaximaMotivo);

        // Propiedades calculadas del dominio: no se guardan
        builder.Ignore(t => t.TienePacienteVinculado);
        builder.Ignore(t => t.EstaActivo);

        // El orden del entre-turno no se repite en el día del odontólogo.
        // MySQL admite varios NULL en un índice único, así que los programados (sin orden) no chocan.
        // Este índice también sirve para buscar la agenda por odontólogo y fecha (prefijo del índice).
        builder.HasIndex(t => new { t.OdontologoId, t.Fecha, t.OrdenEntreTurno }).IsUnique();

        // Referencias a otros módulos solo por Id, sin navegación
        builder.HasOne<OdontologoPerfil>()
            .WithMany()
            .HasForeignKey(t => t.OdontologoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PacientePerfil>()
            .WithMany()
            .HasForeignKey(t => t.PacienteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

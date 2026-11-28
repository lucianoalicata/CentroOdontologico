using Clinica.Dominio.HistoriaClinica;
using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.HistoriaClinica;

public class HistoriaClinicaPacienteConfig : IEntityTypeConfiguration<HistoriaClinicaPaciente>
{
    public void Configure(EntityTypeBuilder<HistoriaClinicaPaciente> builder)
    {
        builder.ToTable("HistoriasClinicas");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.AlertasMedicas).HasMaxLength(HistoriaClinicaPaciente.LongitudMaximaAlertas);

        // Una historia clínica por paciente (la relación uno a uno crea el índice único sobre PacienteId)
        builder.HasOne<PacientePerfil>()
            .WithOne()
            .HasForeignKey<HistoriaClinicaPaciente>(h => h.PacienteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.Entradas)
            .WithOne()
            .HasForeignKey(e => e.HistoriaClinicaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Mapeo del backing field para la colección inmutable
        builder.Navigation(h => h.Entradas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

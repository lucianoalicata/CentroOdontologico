using Clinica.Dominio.Identidad;
using Clinica.Dominio.Turnos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.Turnos;

public class HorarioDisponibleConfig : IEntityTypeConfiguration<HorarioDisponible>
{
    public void Configure(EntityTypeBuilder<HorarioDisponible> builder)
    {
        builder.ToTable("HorariosDisponibles");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.DiaSemana).IsRequired().HasConversion<string>().HasMaxLength(15);

        builder.HasIndex(h => new { h.OdontologoId, h.DiaSemana });

        builder.HasOne<OdontologoPerfil>()
            .WithMany()
            .HasForeignKey(h => h.OdontologoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

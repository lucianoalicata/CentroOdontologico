using Clinica.Dominio.HistoriaClinica;
using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.HistoriaClinica;

public class EntradaHistoriaClinicaConfig : IEntityTypeConfiguration<EntradaHistoriaClinica>
{
    public void Configure(EntityTypeBuilder<EntradaHistoriaClinica> builder)
    {
        builder.ToTable("EntradasHistoriaClinica");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Comentario).HasMaxLength(EntradaHistoriaClinica.LongitudMaximaComentario);

        // Listado cronológico de una historia
        builder.HasIndex(e => new { e.HistoriaClinicaId, e.FechaCreacion });

        // Autor de la entrada
        builder.HasOne<OdontologoPerfil>()
            .WithMany()
            .HasForeignKey(e => e.OdontologoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Una corrección referencia a la entrada anterior (nunca se borra ni se modifica)
        builder.HasOne<EntradaHistoriaClinica>()
            .WithMany()
            .HasForeignKey(e => e.CorrigeEntradaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Prestaciones)
            .WithOne()
            .HasForeignKey(p => p.EntradaHistoriaClinicaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Mapeo del backing field para la colección inmutable
        builder.Navigation(e => e.Prestaciones)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

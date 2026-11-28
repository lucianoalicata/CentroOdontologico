using Clinica.Dominio.HistoriaClinica;
using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.HistoriaClinica;

public class ArchivoAdjuntoConfig : IEntityTypeConfiguration<ArchivoAdjunto>
{
    public void Configure(EntityTypeBuilder<ArchivoAdjunto> builder)
    {
        builder.ToTable("ArchivosAdjuntos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.NombreOriginal).IsRequired().HasMaxLength(ArchivoAdjunto.LongitudMaximaNombre);
        builder.Property(a => a.TipoContenido).IsRequired().HasMaxLength(ArchivoAdjunto.LongitudMaximaTipoContenido);
        builder.Property(a => a.Tipo).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ClaveAlmacenamiento).IsRequired().HasMaxLength(ArchivoAdjunto.LongitudMaximaClave);

        // Un registro por archivo guardado
        builder.HasIndex(a => a.ClaveAlmacenamiento).IsUnique();

        builder.HasOne<EntradaHistoriaClinica>()
            .WithMany()
            .HasForeignKey(a => a.EntradaHistoriaClinicaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(a => a.SubidoPorUsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

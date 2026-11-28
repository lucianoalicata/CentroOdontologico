using Clinica.Dominio.HistoriaClinica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.HistoriaClinica;

public class PrestacionConfig : IEntityTypeConfiguration<Prestacion>
{
    public void Configure(EntityTypeBuilder<Prestacion> builder)
    {
        builder.ToTable("Prestaciones");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(Prestacion.LongitudMaximaCodigo);
        builder.Property(p => p.Titulo).IsRequired().HasMaxLength(Prestacion.LongitudMaximaTitulo);
        builder.Property(p => p.Categoria).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.PrecioParticular).HasPrecision(18, 2);

        // Índice único normal: convive con la reactivación del borrado lógico
        builder.HasIndex(p => p.Codigo).IsUnique();
    }
}

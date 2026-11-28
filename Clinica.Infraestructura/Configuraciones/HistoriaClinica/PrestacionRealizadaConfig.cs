using Clinica.Dominio.HistoriaClinica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.HistoriaClinica;

public class PrestacionRealizadaConfig : IEntityTypeConfiguration<PrestacionRealizada>
{
    public void Configure(EntityTypeBuilder<PrestacionRealizada> builder)
    {
        builder.ToTable("PrestacionesRealizadas");
        builder.HasKey(p => p.Id);

        // Copia de los datos del catálogo al momento de la atención: no hay clave foránea a Prestaciones
        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(PrestacionRealizada.LongitudMaximaCodigo);
        builder.Property(p => p.Titulo).IsRequired().HasMaxLength(PrestacionRealizada.LongitudMaximaTitulo);
        builder.Property(p => p.PrecioUnitario).HasPrecision(18, 2);

        // Propiedad calculada: no es una columna
        builder.Ignore(p => p.Subtotal);
    }
}

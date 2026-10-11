using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.Identidad;

public class PacientePerfilConfig : IEntityTypeConfiguration<PacientePerfil>
{
    public void Configure(EntityTypeBuilder<PacientePerfil> builder)
    {
        builder.ToTable("Pacientes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Dni).IsRequired().HasMaxLength(8);
        builder.Property(p => p.Telefono).IsRequired().HasMaxLength(30);

        builder.HasIndex(p => p.Dni).IsUnique();

        // 1 a 0..1 opcional con Usuario
        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<PacientePerfil>(p => p.UsuarioId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
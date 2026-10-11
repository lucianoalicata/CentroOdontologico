using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.Identidad;

public class SecretariaPerfilConfig : IEntityTypeConfiguration<SecretariaPerfil>
{
    public void Configure(EntityTypeBuilder<SecretariaPerfil> builder)
    {
        builder.ToTable("Secretarias");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Legajo).IsRequired().HasMaxLength(30);

        builder.HasIndex(s => s.Legajo).IsUnique();

        // Relación 1 a 1: toda secretaria tiene un Usuario (FK requerida y única)
        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<SecretariaPerfil>(s => s.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
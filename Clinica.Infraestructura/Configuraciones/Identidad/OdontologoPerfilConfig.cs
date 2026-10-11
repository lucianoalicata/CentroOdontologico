using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.Identidad;

public class OdontologoPerfilConfig : IEntityTypeConfiguration<OdontologoPerfil>
{
    public void Configure(EntityTypeBuilder<OdontologoPerfil> builder)
    {
        builder.ToTable("Odontologos");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Matricula).IsRequired().HasMaxLength(30);
        // Sin IsRequired: sigue la nullabilidad de la propiedad en el dominio
        builder.Property(o => o.Especialidad).HasMaxLength(100);

        builder.HasIndex(o => o.Matricula).IsUnique();

        // Relación 1 a 1: todo odontólogo tiene un Usuario (FK requerida y única)
        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<OdontologoPerfil>(o => o.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
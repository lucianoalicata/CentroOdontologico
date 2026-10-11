using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Configuraciones.Identidad;

public class UsuarioRolConfig : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuarioRoles");

        builder.HasKey(ur => new { ur.UsuarioId, ur.Rol });

        builder.Property(ur => ur.Rol)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne<Usuario>()
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
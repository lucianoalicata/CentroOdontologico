using System.Linq.Expressions;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Persistencia;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<PacientePerfil> Pacientes => Set<PacientePerfil>();
    public DbSet<OdontologoPerfil> Odontologos => Set<OdontologoPerfil>();
    public DbSet<SecretariaPerfil> Secretarias => Set<SecretariaPerfil>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las clases IEntityTypeConfiguration del proyecto
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicaDbContext).Assembly);

        // Global Query Filter para Soft Delete (IBorradoLogico)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IBorradoLogico).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IBorradoLogico.Activo));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(true)), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<string>().HaveMaxLength(200);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<DateOnly>().HaveColumnType("date");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ahoraUtc = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entidad>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(Entidad.FechaCreacion)).CurrentValue = ahoraUtc;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Entidad.FechaModificacion)).CurrentValue = ahoraUtc;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
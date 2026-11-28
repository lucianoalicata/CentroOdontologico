using Clinica.Dominio.HistoriaClinica;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Clinica.Tests.HistoriaClinica;

/// <summary>
/// Verifica la configuración de EF Core del módulo. Solo construye el modelo en memoria:
/// no se conecta a ninguna base de datos.
/// </summary>
public class HistoriaClinicaModeloEfTests
{
    private static readonly IModel Modelo = CrearModelo();

    private static DbContextOptions<ClinicaDbContext> CrearOpciones()
    {
        return new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseMySql("Server=localhost;Database=modelo;User=u;Password=p;", new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
    }

    private static IModel CrearModelo()
    {
        using var contexto = new ClinicaDbContext(CrearOpciones());
        return contexto.Model;
    }

    private static IEntityType Tipo<T>() => Modelo.FindEntityType(typeof(T))!;

    [Fact]
    public void Modelo_CuandoSeConstruye_EntoncesMapeaLasTablasDelModulo()
    {
        Assert.Equal("HistoriasClinicas", Tipo<HistoriaClinicaPaciente>().GetTableName());
        Assert.Equal("EntradasHistoriaClinica", Tipo<EntradaHistoriaClinica>().GetTableName());
        Assert.Equal("PrestacionesRealizadas", Tipo<PrestacionRealizada>().GetTableName());
        Assert.Equal("ArchivosAdjuntos", Tipo<ArchivoAdjunto>().GetTableName());
    }

    [Fact]
    public void Modelo_CuandoSeConstruye_EntoncesNoAlteraLaTablaDePrestaciones()
    {
        Assert.Equal("Prestaciones", Tipo<Prestacion>().GetTableName());
    }

    [Fact]
    public void Modelo_CuandoSeDefinenLasRelaciones_EntoncesTodasUsanRestrict()
    {
        var claves = new[]
        {
            Tipo<HistoriaClinicaPaciente>(),
            Tipo<EntradaHistoriaClinica>(),
            Tipo<PrestacionRealizada>(),
            Tipo<ArchivoAdjunto>()
        }.SelectMany(t => t.GetForeignKeys()).ToList();

        Assert.True(claves.Count > 0);
        Assert.All(claves, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void HistoriaClinica_CuandoSeConfigura_EntoncesHayUnaSolaPorPaciente()
    {
        var indiceUnico = Tipo<HistoriaClinicaPaciente>().GetIndexes().Any(i =>
            i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(HistoriaClinicaPaciente.PacienteId) }));

        Assert.True(indiceUnico);
    }

    [Fact]
    public void Colecciones_CuandoSeConfiguran_EntoncesSonNavegacionesDeColeccion()
    {
        var entradas = Tipo<HistoriaClinicaPaciente>().FindNavigation(nameof(HistoriaClinicaPaciente.Entradas));
        var prestaciones = Tipo<EntradaHistoriaClinica>().FindNavigation(nameof(EntradaHistoriaClinica.Prestaciones));

        Assert.NotNull(entradas);
        Assert.True(entradas!.IsCollection);
        Assert.NotNull(prestaciones);
        Assert.True(prestaciones!.IsCollection);
    }

    [Fact]
    public void PrestacionRealizada_CuandoSeConfigura_EntoncesNoTieneClaveForaneaAlCatalogoNiColumnaSubtotal()
    {
        var tipo = Tipo<PrestacionRealizada>();

        Assert.Empty(tipo.GetForeignKeys().Where(fk => fk.PrincipalEntityType.ClrType == typeof(Prestacion)));
        Assert.Null(tipo.FindProperty(nameof(PrestacionRealizada.Subtotal)));
    }

    [Fact]
    public void Textos_CuandoSeConfiguran_EntoncesUsanLasLongitudesDelDominio()
    {
        Assert.Equal((int?)HistoriaClinicaPaciente.LongitudMaximaAlertas,
            Tipo<HistoriaClinicaPaciente>().FindProperty(nameof(HistoriaClinicaPaciente.AlertasMedicas))!.GetMaxLength());
        Assert.Equal((int?)EntradaHistoriaClinica.LongitudMaximaComentario,
            Tipo<EntradaHistoriaClinica>().FindProperty(nameof(EntradaHistoriaClinica.Comentario))!.GetMaxLength());
        Assert.Equal((int?)PrestacionRealizada.LongitudMaximaCodigo,
            Tipo<PrestacionRealizada>().FindProperty(nameof(PrestacionRealizada.Codigo))!.GetMaxLength());
        Assert.Equal((int?)PrestacionRealizada.LongitudMaximaTitulo,
            Tipo<PrestacionRealizada>().FindProperty(nameof(PrestacionRealizada.Titulo))!.GetMaxLength());
        Assert.Equal((int?)ArchivoAdjunto.LongitudMaximaNombre,
            Tipo<ArchivoAdjunto>().FindProperty(nameof(ArchivoAdjunto.NombreOriginal))!.GetMaxLength());
        Assert.Equal((int?)ArchivoAdjunto.LongitudMaximaTipoContenido,
            Tipo<ArchivoAdjunto>().FindProperty(nameof(ArchivoAdjunto.TipoContenido))!.GetMaxLength());
        Assert.Equal((int?)ArchivoAdjunto.LongitudMaximaClave,
            Tipo<ArchivoAdjunto>().FindProperty(nameof(ArchivoAdjunto.ClaveAlmacenamiento))!.GetMaxLength());
    }

    [Fact]
    public void Add_CuandoLaHistoriaTieneEntradasYPrestaciones_EntoncesEfLasDetectaComoNuevas()
    {
        using var contexto = new ClinicaDbContext(CrearOpciones());
        var historia = new HistoriaClinicaPaciente(1);
        historia.AgregarEntrada(1, "Control", new[] { new PrestacionRealizada("01.01", "Consulta", 5000m) });

        contexto.Add(historia);

        Assert.Equal(1, contexto.ChangeTracker.Entries<EntradaHistoriaClinica>().Count());
        Assert.Equal(1, contexto.ChangeTracker.Entries<PrestacionRealizada>().Count());
        Assert.All(contexto.ChangeTracker.Entries(), e => Assert.Equal(EntityState.Added, e.State));
    }
}

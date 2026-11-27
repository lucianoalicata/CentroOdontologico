using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Tests.HistoriaClinica;

public class PrestacionTests
{
    private static Prestacion CrearValida() =>
        new("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m);

    [Fact]
    public void Constructor_CuandoEsValida_EntoncesGuardaLosDatosNormalizados()
    {
        var prestacion = new Prestacion(" 02.01 ", " Obturación simple ", CategoriaPrestacion.Operatoria, 45, 18500m, true);

        Assert.Equal("02.01", prestacion.Codigo);
        Assert.Equal("Obturación simple", prestacion.Titulo);
        Assert.Equal(CategoriaPrestacion.Operatoria, prestacion.Categoria);
        Assert.Equal(45, prestacion.DuracionEstimadaMinutos);
        Assert.Equal(18500m, prestacion.PrecioParticular);
        Assert.True(prestacion.RequiereAutorizacionPrevia);
    }

    [Fact]
    public void Constructor_CuandoNoSeIndicaAutorizacion_EntoncesNoRequiereAutorizacionPrevia()
    {
        Assert.False(CrearValida().RequiereAutorizacionPrevia);
    }

    [Fact]
    public void Constructor_CuandoSeCrea_EntoncesQuedaActiva()
    {
        var prestacion = CrearValida();

        Assert.True(prestacion.Activo);
        Assert.Null(prestacion.FechaBaja);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_CuandoElCodigoEstaVacio_EntoncesLanzaDomainException(string codigo)
    {
        Assert.Throws<DomainException>(() =>
            new Prestacion(codigo, "Consulta", CategoriaPrestacion.Consulta, 30, 15000m));
    }

    [Fact]
    public void Constructor_CuandoElCodigoSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var codigo = new string('1', Prestacion.LongitudMaximaCodigo + 1);

        Assert.Throws<DomainException>(() =>
            new Prestacion(codigo, "Consulta", CategoriaPrestacion.Consulta, 30, 15000m));
    }

    [Fact]
    public void Constructor_CuandoElTituloEstaVacio_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Prestacion("01.01", " ", CategoriaPrestacion.Consulta, 30, 15000m));
    }

    [Fact]
    public void Constructor_CuandoElTituloSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var titulo = new string('a', Prestacion.LongitudMaximaTitulo + 1);

        Assert.Throws<DomainException>(() =>
            new Prestacion("01.01", titulo, CategoriaPrestacion.Consulta, 30, 15000m));
    }

    [Fact]
    public void Constructor_CuandoLaCategoriaNoExiste_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Prestacion("01.01", "Consulta", (CategoriaPrestacion)999, 30, 15000m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-15)]
    [InlineData(Prestacion.DuracionMaximaMinutos + 1)]
    public void Constructor_CuandoLaDuracionEstaFueraDeRango_EntoncesLanzaDomainException(int minutos)
    {
        Assert.Throws<DomainException>(() =>
            new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, minutos, 15000m));
    }

    [Fact]
    public void Constructor_CuandoElPrecioEsNegativo_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, -1m));
    }

    [Fact]
    public void Constructor_CuandoElPrecioEsCero_EntoncesLoAcepta()
    {
        var prestacion = new Prestacion("01.01", "Control", CategoriaPrestacion.Consulta, 15, 0m);

        Assert.Equal(0m, prestacion.PrecioParticular);
    }

    [Fact]
    public void Actualizar_CuandoLosDatosSonValidos_EntoncesModificaTodoMenosElCodigo()
    {
        var prestacion = CrearValida();

        prestacion.Actualizar("Consulta de urgencia", CategoriaPrestacion.Cirugia, 60, 25000m, true);

        Assert.Equal("01.01", prestacion.Codigo);
        Assert.Equal("Consulta de urgencia", prestacion.Titulo);
        Assert.Equal(CategoriaPrestacion.Cirugia, prestacion.Categoria);
        Assert.Equal(60, prestacion.DuracionEstimadaMinutos);
        Assert.Equal(25000m, prestacion.PrecioParticular);
        Assert.True(prestacion.RequiereAutorizacionPrevia);
    }

    [Fact]
    public void Actualizar_CuandoLaDuracionNoEsValida_EntoncesLanzaDomainException()
    {
        var prestacion = CrearValida();

        Assert.Throws<DomainException>(() =>
            prestacion.Actualizar("Consulta", CategoriaPrestacion.Consulta, 0, 15000m, false));
    }

    [Fact]
    public void CambiarPrecio_CuandoEsValido_EntoncesActualizaElPrecioParticular()
    {
        var prestacion = CrearValida();

        prestacion.CambiarPrecio(17000m);

        Assert.Equal(17000m, prestacion.PrecioParticular);
    }

    [Fact]
    public void CambiarPrecio_CuandoEsNegativo_EntoncesLanzaDomainException()
    {
        var prestacion = CrearValida();

        Assert.Throws<DomainException>(() => prestacion.CambiarPrecio(-100m));
    }

    [Fact]
    public void Desactivar_CuandoEstaActiva_EntoncesQuedaDadaDeBajaConFecha()
    {
        var prestacion = CrearValida();

        prestacion.Desactivar();

        Assert.False(prestacion.Activo);
        Assert.NotNull(prestacion.FechaBaja);
    }

    [Fact]
    public void Reactivar_CuandoEstabaDadaDeBaja_EntoncesVuelveAEstarActiva()
    {
        var prestacion = CrearValida();
        prestacion.Desactivar();

        prestacion.Reactivar();

        Assert.True(prestacion.Activo);
        Assert.Null(prestacion.FechaBaja);
    }
}

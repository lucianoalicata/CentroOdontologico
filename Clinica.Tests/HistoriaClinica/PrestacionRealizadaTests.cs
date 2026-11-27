using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Tests.HistoriaClinica;

public class PrestacionRealizadaTests
{
    [Fact]
    public void Constructor_CuandoEsValida_EntoncesGuardaLosDatosTalComoSeInformaron()
    {
        var prestacion = new PrestacionRealizada("  01.02 ", " Obturación simple ", 18500m, 2, 16, 6);

        Assert.Equal("01.02", prestacion.Codigo);
        Assert.Equal("Obturación simple", prestacion.Titulo);
        Assert.Equal(18500m, prestacion.PrecioUnitario);
        Assert.Equal(2, prestacion.Cantidad);
        Assert.Equal(16, prestacion.PiezaDental);
        Assert.Equal(6, prestacion.MesesGarantia);
    }

    [Fact]
    public void Subtotal_CuandoHayVariasUnidades_EntoncesMultiplicaPrecioPorCantidad()
    {
        var prestacion = new PrestacionRealizada("01.02", "Obturación simple", 18500m, 3);

        Assert.Equal(55500m, prestacion.Subtotal);
    }

    [Fact]
    public void Constructor_CuandoNoSeIndicanOpcionales_EntoncesCantidadEsUnoYSinPiezaNiGarantia()
    {
        var prestacion = new PrestacionRealizada("01.01", "Consulta", 5000m);

        Assert.Equal(1, prestacion.Cantidad);
        Assert.Null(prestacion.PiezaDental);
        Assert.Null(prestacion.MesesGarantia);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_CuandoElCodigoEstaVacio_EntoncesLanzaDomainException(string codigo)
    {
        Assert.Throws<DomainException>(() => new PrestacionRealizada(codigo, "Consulta", 5000m));
    }

    [Fact]
    public void Constructor_CuandoElTituloEstaVacio_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new PrestacionRealizada("01.01", " ", 5000m));
    }

    [Fact]
    public void Constructor_CuandoElTituloSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var titulo = new string('a', PrestacionRealizada.LongitudMaximaTitulo + 1);

        Assert.Throws<DomainException>(() => new PrestacionRealizada("01.01", titulo, 5000m));
    }

    [Fact]
    public void Constructor_CuandoElPrecioEsNegativo_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new PrestacionRealizada("01.01", "Consulta", -1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Constructor_CuandoLaCantidadEsMenorAUno_EntoncesLanzaDomainException(int cantidad)
    {
        Assert.Throws<DomainException>(() => new PrestacionRealizada("01.01", "Consulta", 5000m, cantidad));
    }

    [Theory]
    [InlineData(11)]
    [InlineData(48)]
    [InlineData(51)]
    [InlineData(85)]
    public void Constructor_CuandoLaPiezaEsFdiValida_EntoncesLaAcepta(int pieza)
    {
        var prestacion = new PrestacionRealizada("01.01", "Consulta", 5000m, 1, pieza);

        Assert.Equal(pieza, prestacion.PiezaDental);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(19)]
    [InlineData(49)]
    [InlineData(56)]
    [InlineData(86)]
    [InlineData(99)]
    [InlineData(116)]
    public void Constructor_CuandoLaPiezaNoEsFdiValida_EntoncesLanzaDomainException(int pieza)
    {
        Assert.Throws<DomainException>(() => new PrestacionRealizada("01.01", "Consulta", 5000m, 1, pieza));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_CuandoLaGarantiaNoEsPositiva_EntoncesLanzaDomainException(int meses)
    {
        Assert.Throws<DomainException>(() => new PrestacionRealizada("01.01", "Consulta", 5000m, 1, null, meses));
    }
}

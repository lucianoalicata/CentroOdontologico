using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Tests.HistoriaClinica;

public class EntradaHistoriaClinicaTests
{
    [Fact]
    public void Constructor_CuandoSoloHayComentario_EntoncesEsValida()
    {
        var entrada = new EntradaHistoriaClinica(3, "  Control de rutina sin hallazgos.  ");

        Assert.Equal(3, entrada.OdontologoId);
        Assert.Equal("Control de rutina sin hallazgos.", entrada.Comentario);
        Assert.Empty(entrada.Prestaciones);
        Assert.Null(entrada.CorrigeEntradaId);
    }

    [Fact]
    public void Constructor_CuandoSoloHayPrestaciones_EntoncesEsValida()
    {
        var prestaciones = new[] { new PrestacionRealizada("01.01", "Consulta", 5000m) };

        var entrada = new EntradaHistoriaClinica(3, null, prestaciones);

        Assert.Null(entrada.Comentario);
        Assert.Single(entrada.Prestaciones);
    }

    [Fact]
    public void Constructor_CuandoElComentarioEstaEnBlancoYSinPrestaciones_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new EntradaHistoriaClinica(3, "   "));
    }

    [Fact]
    public void Constructor_CuandoNoHayComentarioNiPrestaciones_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new EntradaHistoriaClinica(3, null, Array.Empty<PrestacionRealizada>()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_CuandoElOdontologoNoEsValido_EntoncesLanzaDomainException(int odontologoId)
    {
        Assert.Throws<DomainException>(() => new EntradaHistoriaClinica(odontologoId, "Control"));
    }

    [Fact]
    public void Constructor_CuandoElComentarioSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var comentario = new string('a', EntradaHistoriaClinica.LongitudMaximaComentario + 1);

        Assert.Throws<DomainException>(() => new EntradaHistoriaClinica(3, comentario));
    }

    [Fact]
    public void Constructor_CuandoCorrigeUnaEntrada_EntoncesGuardaLaReferencia()
    {
        var entrada = new EntradaHistoriaClinica(3, "Corrección del diagnóstico anterior.", null, 7);

        Assert.Equal(7, entrada.CorrigeEntradaId);
    }

    [Fact]
    public void Constructor_CuandoLaEntradaACorregirNoEsValida_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => new EntradaHistoriaClinica(3, "Corrección", null, 0));
    }

    [Fact]
    public void Prestaciones_CuandoSeIntentaAgregarDesdeAfuera_EntoncesNoSePermite()
    {
        var entrada = new EntradaHistoriaClinica(3, "Control");
        var coleccion = (ICollection<PrestacionRealizada>)entrada.Prestaciones;

        Assert.Throws<NotSupportedException>(() => coleccion.Add(new PrestacionRealizada("01.01", "Consulta", 5000m)));
    }
}

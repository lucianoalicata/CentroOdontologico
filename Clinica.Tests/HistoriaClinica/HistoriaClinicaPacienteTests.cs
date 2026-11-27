using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Tests.HistoriaClinica;

public class HistoriaClinicaPacienteTests
{
    [Fact]
    public void Constructor_CuandoElPacienteEsValido_EntoncesQuedaSinEntradasNiAlertas()
    {
        var historia = new HistoriaClinicaPaciente(5);

        Assert.Equal(5, historia.PacienteId);
        Assert.Null(historia.AlertasMedicas);
        Assert.Empty(historia.Entradas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void Constructor_CuandoElPacienteNoEsValido_EntoncesLanzaDomainException(int pacienteId)
    {
        Assert.Throws<DomainException>(() => new HistoriaClinicaPaciente(pacienteId));
    }

    [Fact]
    public void ActualizarAlertasMedicas_CuandoHayTexto_EntoncesLoGuardaSinEspaciosSobrantes()
    {
        var historia = new HistoriaClinicaPaciente(5);

        historia.ActualizarAlertasMedicas("  Alergia a la penicilina.  ");

        Assert.Equal("Alergia a la penicilina.", historia.AlertasMedicas);
    }

    [Fact]
    public void ActualizarAlertasMedicas_CuandoEstaEnBlanco_EntoncesLimpiaLasAlertas()
    {
        var historia = new HistoriaClinicaPaciente(5);
        historia.ActualizarAlertasMedicas("Hipertensión.");

        historia.ActualizarAlertasMedicas("   ");

        Assert.Null(historia.AlertasMedicas);
    }

    [Fact]
    public void ActualizarAlertasMedicas_CuandoSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var historia = new HistoriaClinicaPaciente(5);
        var texto = new string('a', HistoriaClinicaPaciente.LongitudMaximaAlertas + 1);

        Assert.Throws<DomainException>(() => historia.ActualizarAlertasMedicas(texto));
    }

    [Fact]
    public void AgregarEntrada_CuandoEsValida_EntoncesQuedaEnLaHistoria()
    {
        var historia = new HistoriaClinicaPaciente(5);
        var prestaciones = new[] { new PrestacionRealizada("01.02", "Obturación simple", 18500m, 1, 16) };

        var entrada = historia.AgregarEntrada(3, "Caries mesial pieza 16.", prestaciones);

        Assert.Single(historia.Entradas);
        Assert.Same(entrada, historia.Entradas.Single());
        Assert.Equal(3, entrada.OdontologoId);
        Assert.Single(entrada.Prestaciones);
    }

    [Fact]
    public void AgregarEntrada_CuandoLaEntradaEsInvalida_EntoncesLanzaDomainExceptionYNoAgrega()
    {
        var historia = new HistoriaClinicaPaciente(5);

        Assert.Throws<DomainException>(() => historia.AgregarEntrada(3, null));
        Assert.Empty(historia.Entradas);
    }

    [Fact]
    public void AgregarEntrada_CuandoCorrigeUnaEntradaDeLaMismaHistoria_EntoncesGuardaLaReferencia()
    {
        var historia = new HistoriaClinicaPaciente(5);
        var original = ConId(historia.AgregarEntrada(3, "Diagnóstico inicial."), 10);

        var correccion = historia.AgregarEntrada(3, "Corrijo el diagnóstico: era pieza 26.", null, original.Id);

        Assert.Equal(10, correccion.CorrigeEntradaId);
        Assert.Equal(2, historia.Entradas.Count);
    }

    [Fact]
    public void AgregarEntrada_CuandoCorrigeUnaEntradaQueNoPerteneceALaHistoria_EntoncesLanzaDomainException()
    {
        var historia = new HistoriaClinicaPaciente(5);
        historia.AgregarEntrada(3, "Diagnóstico inicial.");

        Assert.Throws<DomainException>(() => historia.AgregarEntrada(3, "Corrección", null, 999));
        Assert.Single(historia.Entradas);
    }

    [Fact]
    public void Entradas_CuandoSeIntentaAgregarDesdeAfuera_EntoncesNoSePermite()
    {
        var historia = new HistoriaClinicaPaciente(5);
        var coleccion = (ICollection<EntradaHistoriaClinica>)historia.Entradas;

        Assert.Throws<NotSupportedException>(() => coleccion.Add(new EntradaHistoriaClinica(3, "Control")));
    }

    private static T ConId<T>(T entidad, int id) where T : Entidad
    {
        typeof(Entidad).GetProperty(nameof(Entidad.Id))!.SetValue(entidad, id);
        return entidad;
    }
}

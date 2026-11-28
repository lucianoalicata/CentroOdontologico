using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

public class HorarioDisponibleTests
{
    private static HorarioDisponible Horario(
        string inicio = "09:00",
        string fin = "13:00",
        DiaSemana dia = DiaSemana.Lunes,
        int duracion = 30,
        int odontologoId = 1) =>
        new(odontologoId, dia, TimeOnly.Parse(inicio), TimeOnly.Parse(fin), duracion);

    private static HorarioDisponible ConId(HorarioDisponible horario, int id)
    {
        typeof(Entidad).GetProperty(nameof(Entidad.Id))!.SetValue(horario, id);
        return horario;
    }

    // ---------- Creación ----------

    [Fact]
    public void Constructor_CuandoLosDatosSonValidos_EntoncesQuedaActivoConLaFranja()
    {
        var horario = Horario();

        Assert.Equal(1, horario.OdontologoId);
        Assert.Equal(DiaSemana.Lunes, horario.DiaSemana);
        Assert.Equal(new TimeOnly(9, 0), horario.HoraInicio);
        Assert.Equal(new TimeOnly(13, 0), horario.HoraFin);
        Assert.Equal(30, horario.DuracionTurnoMinutos);
        Assert.True(horario.Activo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_CuandoElOdontologoNoEsValido_EntoncesLanzaDomainException(int odontologoId)
    {
        Assert.Throws<DomainException>(() => Horario(odontologoId: odontologoId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void Constructor_CuandoElDiaNoEsValido_EntoncesLanzaDomainException(int dia)
    {
        Assert.Throws<DomainException>(() => Horario(dia: (DiaSemana)dia));
    }

    [Theory]
    [InlineData("13:00", "13:00")]
    [InlineData("13:00", "09:00")]
    public void Constructor_CuandoLaHoraFinNoEsPosterior_EntoncesLanzaDomainException(string inicio, string fin)
    {
        Assert.Throws<DomainException>(() => Horario(inicio, fin));
    }

    [Theory]
    [InlineData(9)]
    [InlineData(121)]
    [InlineData(0)]
    public void Constructor_CuandoLaDuracionEstaFueraDeRango_EntoncesLanzaDomainException(int duracion)
    {
        Assert.Throws<DomainException>(() => Horario(duracion: duracion));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(120)]
    public void Constructor_CuandoLaDuracionEstaEnElLimite_EntoncesEsValido(int duracion)
    {
        var horario = Horario("08:00", "12:00", duracion: duracion);

        Assert.Equal(duracion, horario.DuracionTurnoMinutos);
    }

    [Fact]
    public void Constructor_CuandoLaFranjaEsMasCortaQueUnTurno_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Horario("09:00", "09:20", duracion: 30));
    }

    [Fact]
    public void Constructor_CuandoLaFranjaDuraExactamenteUnTurno_EntoncesEsValido()
    {
        var horario = Horario("09:00", "09:30", duracion: 30);

        Assert.Equal(new TimeOnly(9, 30), horario.HoraFin);
    }

    // ---------- Actualización ----------

    [Fact]
    public void Actualizar_CuandoLosDatosSonValidos_EntoncesCambiaLaFranja()
    {
        var horario = Horario();

        horario.Actualizar(DiaSemana.Viernes, new TimeOnly(14, 0), new TimeOnly(18, 0), 45);

        Assert.Equal(DiaSemana.Viernes, horario.DiaSemana);
        Assert.Equal(new TimeOnly(14, 0), horario.HoraInicio);
        Assert.Equal(new TimeOnly(18, 0), horario.HoraFin);
        Assert.Equal(45, horario.DuracionTurnoMinutos);
        Assert.Equal(1, horario.OdontologoId);
    }

    [Fact]
    public void Actualizar_CuandoLosDatosSonInvalidos_EntoncesNoModificaNada()
    {
        var horario = Horario();

        Assert.Throws<DomainException>(() =>
            horario.Actualizar(DiaSemana.Viernes, new TimeOnly(18, 0), new TimeOnly(14, 0), 45));

        Assert.Equal(DiaSemana.Lunes, horario.DiaSemana);
        Assert.Equal(new TimeOnly(9, 0), horario.HoraInicio);
        Assert.Equal(new TimeOnly(13, 0), horario.HoraFin);
        Assert.Equal(30, horario.DuracionTurnoMinutos);
    }

    // ---------- Solapamiento ----------

    [Theory]
    [InlineData("09:00", "13:00", "12:00", "15:00", true)]
    [InlineData("09:00", "13:00", "10:00", "11:00", true)]
    [InlineData("09:00", "13:00", "08:00", "09:30", true)]
    [InlineData("09:00", "13:00", "09:00", "13:00", true)]
    [InlineData("09:00", "13:00", "13:00", "17:00", false)]
    [InlineData("09:00", "13:00", "07:00", "09:00", false)]
    [InlineData("09:00", "13:00", "14:00", "18:00", false)]
    public void SeSolapaCon_CuandoSonDelMismoDiaYOdontologo_EntoncesDetectaSuperposicion(
        string inicioA, string finA, string inicioB, string finB, bool esperado)
    {
        var a = Horario(inicioA, finA);
        var b = Horario(inicioB, finB);

        Assert.Equal(esperado, a.SeSolapaCon(b));
        Assert.Equal(esperado, b.SeSolapaCon(a));
    }

    [Fact]
    public void SeSolapaCon_CuandoSonDeDistintoDia_EntoncesNoSeSolapan()
    {
        Assert.False(Horario(dia: DiaSemana.Lunes).SeSolapaCon(Horario(dia: DiaSemana.Martes)));
    }

    [Fact]
    public void SeSolapaCon_CuandoSonDeDistintoOdontologo_EntoncesNoSeSolapan()
    {
        Assert.False(Horario(odontologoId: 1).SeSolapaCon(Horario(odontologoId: 2)));
    }

    [Fact]
    public void SeSolapaCon_CuandoElOtroEstaDadoDeBaja_EntoncesNoSeSolapan()
    {
        var dadoDeBaja = Horario();
        dadoDeBaja.Desactivar();

        Assert.False(Horario().SeSolapaCon(dadoDeBaja));
    }

    [Fact]
    public void SeSolapaCon_CuandoEsLaMismaFranja_EntoncesNoSeSolapa()
    {
        var horario = Horario();

        Assert.False(horario.SeSolapaCon(horario));
    }

    [Fact]
    public void ValidarQueNoSeSolapaCon_CuandoHayConflicto_EntoncesLanzaDomainExceptionConElHorario()
    {
        var existentes = new[] { ConId(Horario("09:00", "13:00"), 1) };
        var nuevo = Horario("12:00", "15:00");

        var ex = Assert.Throws<DomainException>(() => nuevo.ValidarQueNoSeSolapaCon(existentes));

        Assert.Contains("lunes", ex.Message);
        Assert.Contains("09:00 a 13:00", ex.Message);
    }

    [Fact]
    public void ValidarQueNoSeSolapaCon_CuandoNoHayConflicto_EntoncesNoLanza()
    {
        var existentes = new[]
        {
            ConId(Horario("09:00", "13:00"), 1),
            ConId(Horario("09:00", "13:00", dia: DiaSemana.Martes), 2)
        };
        var nuevo = Horario("14:00", "18:00");

        var ex = Record.Exception(() => nuevo.ValidarQueNoSeSolapaCon(existentes));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidarQueNoSeSolapaCon_CuandoSeEditaUnaFranjaQueEstaEnLaLista_EntoncesSeIgnoraASiMisma()
    {
        // Simula la edición: el repositorio devuelve una instancia distinta con el mismo Id.
        var guardada = ConId(Horario("09:00", "13:00"), 5);
        var editada = ConId(Horario("09:00", "13:00"), 5);
        editada.Actualizar(DiaSemana.Lunes, new TimeOnly(9, 0), new TimeOnly(14, 0), 30);

        var ex = Record.Exception(() => editada.ValidarQueNoSeSolapaCon(new[] { guardada }));

        Assert.Null(ex);
    }

    // ---------- Día de una fecha ----------

    [Theory]
    [InlineData(2026, 11, 30, DiaSemana.Lunes)]
    [InlineData(2026, 12, 2, DiaSemana.Miercoles)]
    [InlineData(2026, 12, 5, DiaSemana.Sabado)]
    [InlineData(2026, 12, 6, DiaSemana.Domingo)]
    public void DiaDe_CuandoSeIndicaUnaFecha_EntoncesDevuelveElDiaConNumeracionDelDominio(
        int anio, int mes, int dia, DiaSemana esperado)
    {
        Assert.Equal(esperado, HorarioDisponible.DiaDe(new DateOnly(anio, mes, dia)));
    }
}

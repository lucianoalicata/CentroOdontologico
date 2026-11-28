using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

public class CalculadoraDisponibilidadTests
{
    private const int Odontologo = 1;
    private static readonly DateOnly Lunes = new(2026, 11, 30);

    private static HorarioDisponible Horario(
        string inicio, string fin, int duracion = 30,
        DiaSemana dia = DiaSemana.Lunes, int odontologoId = Odontologo) =>
        new(odontologoId, dia, TimeOnly.Parse(inicio), TimeOnly.Parse(fin), duracion);

    private static Turno Programado(
        string inicio, string fin, DateOnly? fecha = null, int odontologoId = Odontologo) =>
        Turno.CrearProgramado(
            odontologoId, 10, fecha ?? Lunes, TimeOnly.Parse(inicio), TimeOnly.Parse(fin),
            null, OrigenCreacion.TelefonoSecretaria, 99);

    private static string[] Inicios(IEnumerable<FranjaHoraria> franjas) =>
        franjas.Select(f => f.Inicio.ToString("HH\\:mm")).ToArray();

    private static IReadOnlyList<FranjaHoraria> Calcular(
        IEnumerable<HorarioDisponible> horarios, IEnumerable<Turno>? turnos = null, DateOnly? fecha = null) =>
        CalculadoraDisponibilidad.FranjasLibres(Odontologo, fecha ?? Lunes, horarios, turnos ?? Array.Empty<Turno>());

    // ---------- Generación de franjas ----------

    [Fact]
    public void GenerarFranjas_CuandoLaFranjaEsDivisible_EntoncesDevuelveTurnosConsecutivos()
    {
        var franjas = CalculadoraDisponibilidad.GenerarFranjas(Horario("09:00", "11:00")).ToList();

        Assert.Equal(new[] { "09:00", "09:30", "10:00", "10:30" }, Inicios(franjas));
        Assert.Equal(new TimeOnly(11, 0), franjas[^1].Fin);
    }

    [Fact]
    public void GenerarFranjas_CuandoSobraUnRestoMenorAUnTurno_EntoncesLoDescarta()
    {
        var franjas = CalculadoraDisponibilidad.GenerarFranjas(Horario("09:00", "10:45")).ToList();

        Assert.Equal(new[] { "09:00", "09:30", "10:00" }, Inicios(franjas));
    }

    [Fact]
    public void GenerarFranjas_CuandoElHorarioTerminaCercaDeMedianoche_EntoncesNoDaLaVuelta()
    {
        var franjas = CalculadoraDisponibilidad.GenerarFranjas(Horario("22:00", "23:59", duracion: 60)).ToList();

        Assert.Equal(new[] { "22:00" }, Inicios(franjas));
    }

    // ---------- Franjas libres ----------

    [Fact]
    public void FranjasLibres_CuandoNoHayTurnos_EntoncesDevuelveTodasLasFranjasDelDia()
    {
        var libres = Calcular(new[] { Horario("09:00", "11:00") });

        Assert.Equal(new[] { "09:00", "09:30", "10:00", "10:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoHayUnTurnoProgramado_EntoncesDescuentaEsaFranja()
    {
        var libres = Calcular(
            new[] { Horario("09:00", "11:00") },
            new[] { Programado("09:30", "10:00") });

        Assert.Equal(new[] { "09:00", "10:00", "10:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoUnTurnoOcupaParteDeDosFranjas_EntoncesDescuentaAmbas()
    {
        // Turno cargado con otra duración, por ejemplo antes de que el odontólogo cambiara su horario.
        var libres = Calcular(
            new[] { Horario("09:00", "11:00") },
            new[] { Programado("09:15", "09:45") });

        Assert.Equal(new[] { "10:00", "10:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoElTurnoEstaCancelado_EntoncesLaFranjaQuedaLibre()
    {
        var cancelado = Programado("09:30", "10:00");
        cancelado.Cancelar("El paciente avisó", 7);

        var libres = Calcular(new[] { Horario("09:00", "10:00") }, new[] { cancelado });

        Assert.Equal(new[] { "09:00", "09:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoElTurnoEstaEnCurso_EntoncesLaFranjaSigueOcupada()
    {
        var enCurso = Programado("09:00", "09:30");
        enCurso.Iniciar();

        var libres = Calcular(new[] { Horario("09:00", "10:00") }, new[] { enCurso });

        Assert.Equal(new[] { "09:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoElTurnoFueAusente_EntoncesLaFranjaQuedaLibre()
    {
        var ausente = Programado("09:00", "09:30");
        ausente.MarcarAusente();

        var libres = Calcular(new[] { Horario("09:00", "10:00") }, new[] { ausente });

        Assert.Equal(new[] { "09:00", "09:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoHayEntreTurnos_EntoncesNoOcupanFranja()
    {
        var entreTurno = Turno.CrearEntreTurno(
            Odontologo, Lunes, 1, null, "Juan", "1155554444", null, null, OrigenCreacion.PresencialSecretaria, 99);

        var libres = Calcular(new[] { Horario("09:00", "10:00") }, new[] { entreTurno });

        Assert.Equal(new[] { "09:00", "09:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoElTurnoEsDeOtroDiaOOtroOdontologo_EntoncesNoLoDescuenta()
    {
        var turnos = new[]
        {
            Programado("09:00", "09:30", fecha: Lunes.AddDays(7)),
            Programado("09:30", "10:00", odontologoId: 2)
        };

        var libres = Calcular(new[] { Horario("09:00", "10:00") }, turnos);

        Assert.Equal(new[] { "09:00", "09:30" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoHayVariosHorariosEnElDia_EntoncesLosUneOrdenados()
    {
        var horarios = new[]
        {
            Horario("14:00", "15:00", duracion: 60),
            Horario("09:00", "10:00")
        };

        var libres = Calcular(horarios);

        Assert.Equal(new[] { "09:00", "09:30", "14:00" }, Inicios(libres));
    }

    [Fact]
    public void FranjasLibres_CuandoElHorarioEsDeOtroDia_EntoncesNoDevuelveFranjas()
    {
        var libres = Calcular(new[] { Horario("09:00", "13:00", dia: DiaSemana.Martes) });

        Assert.Empty(libres);
    }

    [Fact]
    public void FranjasLibres_CuandoElHorarioEsDeOtroOdontologo_EntoncesNoDevuelveFranjas()
    {
        var libres = Calcular(new[] { Horario("09:00", "13:00", odontologoId: 2) });

        Assert.Empty(libres);
    }

    [Fact]
    public void FranjasLibres_CuandoElHorarioEstaDadoDeBaja_EntoncesNoDevuelveFranjas()
    {
        var horario = Horario("09:00", "13:00");
        horario.Desactivar();

        Assert.Empty(Calcular(new[] { horario }));
    }

    [Fact]
    public void FranjasLibres_CuandoTodoEstaOcupado_EntoncesDevuelveListaVacia()
    {
        var libres = Calcular(
            new[] { Horario("09:00", "10:00") },
            new[] { Programado("09:00", "09:30"), Programado("09:30", "10:00") });

        Assert.Empty(libres);
    }

    [Fact]
    public void FranjasLibres_CuandoLaFechaEsDomingo_EntoncesUsaLosHorariosDelDomingo()
    {
        var domingo = new DateOnly(2026, 12, 6);

        var libres = Calcular(new[] { Horario("10:00", "11:00", dia: DiaSemana.Domingo) }, fecha: domingo);

        Assert.Equal(new[] { "10:00", "10:30" }, Inicios(libres));
    }
}

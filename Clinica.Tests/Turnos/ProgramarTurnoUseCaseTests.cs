using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Turnos.Dtos;
using Clinica.Aplicacion.Turnos.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

public class ProgramarTurnoUseCaseTests
{
    private const int Odontologo = 1;
    private const int Paciente = 10;
    private static readonly DateOnly Lunes = new(2026, 11, 30);
    private static readonly DateTime ViernesAntes = new(2026, 11, 27, 10, 0, 0);

    private readonly FakeTurnos _turnos = new();
    private readonly FakeHorarios _horarios = new();
    private readonly FakeConsultaPerfiles _perfiles = new();
    private readonly FakeReloj _reloj = new(ViernesAntes);
    private readonly FakeUnitOfWork _uow = new();

    public ProgramarTurnoUseCaseTests()
    {
        _perfiles.OdontologosActivos.Add(Odontologo);
        _perfiles.PacientesActivos.Add(Paciente);
        // Lunes de 09:00 a 11:00, turnos de 30 minutos
        _horarios.Lista.Add(new HorarioDisponible(Odontologo, DiaSemana.Lunes, new TimeOnly(9, 0), new TimeOnly(11, 0), 30));
    }

    private ProgramarTurnoUseCase Crear(FakeUsuarioActual usuario) =>
        new(_turnos, _horarios, _perfiles, usuario, _reloj, _uow);

    private static ProgramarTurnoDto Dto(
        string hora = "09:30",
        int odontologoId = Odontologo,
        int pacienteId = Paciente,
        DateOnly? fecha = null,
        OrigenCreacion? origen = null) =>
        new(odontologoId, pacienteId, fecha ?? Lunes, hora, "Control", origen);

    // ---------- Caso feliz ----------

    [Fact]
    public async Task Ejecutar_CuandoLaSecretariaEligeUnaFranjaLibre_EntoncesCreaElTurnoPendienteYGuarda()
    {
        var resultado = await Crear(FakeUsuarioActual.Secretaria(usuarioId: 50)).EjecutarAsync(Dto("09:30"));

        Assert.Equal(TipoTurno.Programado, resultado.Tipo);
        Assert.Equal(EstadoTurno.Pendiente, resultado.Estado);
        Assert.Equal("09:30", resultado.HoraInicio);
        Assert.Equal("10:00", resultado.HoraFin);
        Assert.Equal(Paciente, resultado.PacienteId);
        Assert.Equal(OrigenCreacion.PresencialSecretaria, resultado.Origen);

        var guardado = Assert.Single(_turnos.Lista);
        Assert.Equal(50, guardado.CreadoPor);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoLaSecretariaIndicaPedidoTelefonico_EntoncesRegistraEseOrigen()
    {
        var resultado = await Crear(FakeUsuarioActual.Secretaria())
            .EjecutarAsync(Dto(origen: OrigenCreacion.TelefonoSecretaria));

        Assert.Equal(OrigenCreacion.TelefonoSecretaria, resultado.Origen);
    }

    [Theory]
    [InlineData(OrigenCreacion.Web)]
    [InlineData(OrigenCreacion.Odontologo)]
    public async Task Ejecutar_CuandoLaSecretariaIndicaUnOrigenQueNoEsDelPersonal_EntoncesLanzaDomainException(OrigenCreacion origen)
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(origen: origen)));
    }

    [Fact]
    public async Task Ejecutar_CuandoLoProgramaElAdministrador_EntoncesSeTrataComoPersonal()
    {
        var resultado = await Crear(FakeUsuarioActual.Administrador()).EjecutarAsync(Dto());

        Assert.Equal(OrigenCreacion.PresencialSecretaria, resultado.Origen);
    }

    // ---------- Permisos ----------

    [Fact]
    public async Task Ejecutar_CuandoElPacienteSacaTurnoParaSiMismo_EntoncesElOrigenEsWeb()
    {
        var resultado = await Crear(FakeUsuarioActual.Paciente(Paciente))
            .EjecutarAsync(Dto(origen: OrigenCreacion.PresencialSecretaria));

        Assert.Equal(OrigenCreacion.Web, resultado.Origen);
    }

    [Fact]
    public async Task Ejecutar_CuandoElPacienteSacaTurnoParaOtro_EntoncesLanzaAccesoDenegado()
    {
        _perfiles.PacientesActivos.Add(11);

        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Paciente(Paciente)).EjecutarAsync(Dto(pacienteId: 11)));
        Assert.Empty(_turnos.Lista);
    }

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoProgramaEnSuAgenda_EntoncesElOrigenEsOdontologo()
    {
        var resultado = await Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(Dto());

        Assert.Equal(OrigenCreacion.Odontologo, resultado.Origen);
    }

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoProgramaEnAgendaAjena_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Odontologo(odontologoId: 2)).EjecutarAsync(Dto()));
    }

    [Fact]
    public async Task Ejecutar_CuandoElUsuarioNoTieneRoles_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.SinRoles()).EjecutarAsync(Dto()));
    }

    // ---------- Disponibilidad ----------

    [Fact]
    public async Task Ejecutar_CuandoLaFranjaYaEstaOcupada_EntoncesLanzaDomainException()
    {
        _turnos.Sembrar(Turno.CrearProgramado(
            Odontologo, 12, Lunes, new TimeOnly(9, 30), new TimeOnly(10, 0), null, OrigenCreacion.Web, 99));

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto("09:30")));

        Assert.Contains("no está disponible", ex.Message);
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoLaFranjaQuedoLibrePorUnaCancelacion_EntoncesPermiteProgramar()
    {
        var cancelado = _turnos.Sembrar(Turno.CrearProgramado(
            Odontologo, 12, Lunes, new TimeOnly(9, 30), new TimeOnly(10, 0), null, OrigenCreacion.Web, 99));
        cancelado.Cancelar("No puede venir", 50);

        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto("09:30"));

        Assert.Equal("09:30", resultado.HoraInicio);
    }

    [Theory]
    [InlineData("08:30")] // antes del horario
    [InlineData("11:00")] // termina el horario
    [InlineData("09:15")] // no coincide con el inicio de una franja
    public async Task Ejecutar_CuandoLaHoraNoEsUnaFranjaDelHorario_EntoncesLanzaDomainException(string hora)
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(hora)));
    }

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoNoAtiendeEseDia_EntoncesLanzaDomainException()
    {
        var martes = Lunes.AddDays(1);

        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(fecha: martes)));
    }

    // ---------- Fecha y hora ----------

    [Fact]
    public async Task Ejecutar_CuandoLaFechaYaPaso_EntoncesLanzaDomainException()
    {
        _reloj.AhoraEnClinica = new DateTime(2026, 12, 1, 8, 0, 0);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto()));

        Assert.Contains("ya pasó", ex.Message);
    }

    [Fact]
    public async Task Ejecutar_CuandoEsHoyYLaHoraYaPaso_EntoncesLanzaDomainException()
    {
        _reloj.AhoraEnClinica = new DateTime(2026, 11, 30, 9, 45, 0);

        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto("09:30")));
    }

    [Fact]
    public async Task Ejecutar_CuandoEsHoyYLaHoraTodaviaNoLlego_EntoncesPermiteProgramar()
    {
        _reloj.AhoraEnClinica = new DateTime(2026, 11, 30, 9, 45, 0);

        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto("10:00"));

        Assert.Equal("10:00", resultado.HoraInicio);
    }

    [Theory]
    [InlineData("")]
    [InlineData("9.30")]
    [InlineData("25:00")]
    [InlineData("nueve")]
    public async Task Ejecutar_CuandoLaHoraTieneFormatoInvalido_EntoncesLanzaDomainException(string hora)
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(hora)));
    }

    // ---------- Paciente y odontólogo ----------

    [Fact]
    public async Task Ejecutar_CuandoElPacienteNoExisteOEstaDadoDeBaja_EntoncesLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(pacienteId: 999)));
    }

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoNoExisteOEstaDadoDeBaja_EntoncesLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(odontologoId: 999)));
    }

    [Theory]
    [InlineData(0, Paciente)]
    [InlineData(Odontologo, 0)]
    public async Task Ejecutar_CuandoLosIdsNoSonValidos_EntoncesLanzaDomainException(int odontologoId, int pacienteId)
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto(odontologoId: odontologoId, pacienteId: pacienteId)));
    }
}

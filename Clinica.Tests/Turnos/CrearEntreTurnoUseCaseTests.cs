using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Turnos.Dtos;
using Clinica.Aplicacion.Turnos.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

public class CrearEntreTurnoUseCaseTests
{
    private const int Odontologo = 1;
    private static readonly DateOnly Lunes = new(2026, 11, 30);

    private readonly FakeTurnos _turnos = new();
    private readonly FakeConsultaPerfiles _perfiles = new();
    private readonly FakeReloj _reloj = new(new DateTime(2026, 11, 30, 10, 0, 0));
    private readonly FakeUnitOfWork _uow = new();

    public CrearEntreTurnoUseCaseTests()
    {
        _perfiles.OdontologosActivos.Add(Odontologo);
        _perfiles.PacientesActivos.Add(10);
    }

    private CrearEntreTurnoUseCase Crear(FakeUsuarioActual usuario) =>
        new(_turnos, _perfiles, usuario, _reloj, _uow);

    private static CrearEntreTurnoDto Provisorio(
        string? dni = null,
        int odontologoId = Odontologo,
        DateOnly? fecha = null,
        OrigenCreacion? origen = null) =>
        new(odontologoId, fecha ?? Lunes, null, "Juan Pérez", "1155554444", dni, "Dolor de muela", "11:00", origen);

    // ---------- Creación y orden correlativo ----------

    [Fact]
    public async Task Ejecutar_CuandoEsElPrimeroDelDia_EntoncesTieneOrdenUnoYGuardaDatosProvisorios()
    {
        var resultado = await Crear(FakeUsuarioActual.Secretaria(usuarioId: 50)).EjecutarAsync(Provisorio());

        var turno = resultado.Turno;
        Assert.Equal(TipoTurno.EntreTurno, turno.Tipo);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        Assert.Equal(1, turno.OrdenEntreTurno);
        Assert.Null(turno.PacienteId);
        Assert.Equal("Juan Pérez", turno.PacienteWalkInNombre);
        Assert.Equal("1155554444", turno.PacienteWalkInTelefono);
        Assert.Null(turno.HoraInicio);
        Assert.Equal(OrigenCreacion.PresencialSecretaria, turno.Origen);
        Assert.Equal(50, Assert.Single(_turnos.Lista).CreadoPor);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoYaHayEntreTurnosEseDia_EntoncesUsaElSiguienteOrden()
    {
        var usecase = Crear(FakeUsuarioActual.Secretaria());

        await usecase.EjecutarAsync(Provisorio());
        await usecase.EjecutarAsync(Provisorio());
        var tercero = await usecase.EjecutarAsync(Provisorio());

        Assert.Equal(3, tercero.Turno.OrdenEntreTurno);
    }

    [Fact]
    public async Task Ejecutar_CuandoUnEntreTurnoAnteriorFueCancelado_EntoncesNoReusaSuNumero()
    {
        var usecase = Crear(FakeUsuarioActual.Secretaria());
        await usecase.EjecutarAsync(Provisorio());
        _turnos.Lista[0].Cancelar("No vino", 50);

        var segundo = await usecase.EjecutarAsync(Provisorio());

        Assert.Equal(2, segundo.Turno.OrdenEntreTurno);
    }

    [Fact]
    public async Task Ejecutar_CuandoLosEntreTurnosSonDeOtroDiaUOtroOdontologo_EntoncesElOrdenEmpiezaEnUno()
    {
        _perfiles.OdontologosActivos.Add(2);
        var usecase = Crear(FakeUsuarioActual.Secretaria());
        await usecase.EjecutarAsync(Provisorio(fecha: Lunes.AddDays(1)));
        await usecase.EjecutarAsync(Provisorio(odontologoId: 2));

        var resultado = await usecase.EjecutarAsync(Provisorio());

        Assert.Equal(1, resultado.Turno.OrdenEntreTurno);
    }

    [Fact]
    public async Task Ejecutar_CuandoSeInformaUnPacienteConFicha_EntoncesLoVincula()
    {
        var dto = new CrearEntreTurnoDto(Odontologo, Lunes, 10, null, null, null, null);

        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(dto);

        Assert.Equal(10, resultado.Turno.PacienteId);
        Assert.Null(resultado.Turno.PacienteWalkInNombre);
        Assert.Null(resultado.PacienteSugerido);
    }

    [Fact]
    public async Task Ejecutar_CuandoElPacienteInformadoNoExiste_EntoncesLanzaNoEncontrado()
    {
        var dto = new CrearEntreTurnoDto(Odontologo, Lunes, 999, null, null, null, null);

        await Assert.ThrowsAsync<NoEncontradoException>(() => Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(dto));
    }

    [Theory]
    [InlineData(null, "1155554444")]
    [InlineData("Juan", "  ")]
    public async Task Ejecutar_CuandoFaltanNombreOTelefonoSinFicha_EntoncesLanzaDomainExceptionYNoGuarda(
        string? nombre, string? telefono)
    {
        var dto = new CrearEntreTurnoDto(Odontologo, Lunes, null, nombre, telefono, null, null);

        await Assert.ThrowsAsync<DomainException>(() => Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(dto));
        Assert.Equal(0, _uow.Guardados);
    }

    // ---------- DNI y sugerencia de ficha ----------

    [Fact]
    public async Task Ejecutar_CuandoElDniYaTieneFicha_EntoncesLaSugiereSinVincularla()
    {
        _perfiles.PacientesPorDni.Add(new PacienteSugeridoDto(10, "Juan", "Pérez", "35123456"));

        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Provisorio(dni: "35.123.456"));

        Assert.NotNull(resultado.PacienteSugerido);
        Assert.Equal(10, resultado.PacienteSugerido!.Id);
        Assert.Null(resultado.Turno.PacienteId);
        Assert.Equal("35123456", resultado.Turno.PacienteWalkInDni);
        Assert.Equal("35123456", _perfiles.UltimoDniBuscado);
    }

    [Fact]
    public async Task Ejecutar_CuandoElDniNoTieneFicha_EntoncesNoHaySugerencia()
    {
        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Provisorio(dni: "40111222"));

        Assert.Null(resultado.PacienteSugerido);
        Assert.Equal("40111222", resultado.Turno.PacienteWalkInDni);
    }

    [Fact]
    public async Task Ejecutar_CuandoNoSeInformaDni_EntoncesNoBuscaFicha()
    {
        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Provisorio(dni: "  "));

        Assert.Null(resultado.PacienteSugerido);
        Assert.Null(resultado.Turno.PacienteWalkInDni);
        Assert.Null(_perfiles.UltimoDniBuscado);
    }

    // ---------- Permisos y origen ----------

    [Fact]
    public async Task Ejecutar_CuandoLaSecretariaIndicaPedidoTelefonico_EntoncesRegistraEseOrigen()
    {
        var resultado = await Crear(FakeUsuarioActual.Secretaria())
            .EjecutarAsync(Provisorio(origen: OrigenCreacion.TelefonoSecretaria));

        Assert.Equal(OrigenCreacion.TelefonoSecretaria, resultado.Turno.Origen);
    }

    [Fact]
    public async Task Ejecutar_CuandoLoRegistraElOdontologoEnSuAgenda_EntoncesElOrigenEsOdontologo()
    {
        var resultado = await Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(Provisorio());

        Assert.Equal(OrigenCreacion.Odontologo, resultado.Turno.Origen);
    }

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoUsaAgendaAjena_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Odontologo(odontologoId: 2)).EjecutarAsync(Provisorio()));
    }

    [Fact]
    public async Task Ejecutar_CuandoLoIntentaUnPaciente_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Paciente(10)).EjecutarAsync(Provisorio()));
        Assert.Empty(_turnos.Lista);
    }

    // ---------- Fecha y odontólogo ----------

    [Fact]
    public async Task Ejecutar_CuandoLaFechaYaPaso_EntoncesLanzaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Provisorio(fecha: Lunes.AddDays(-1))));
    }

    [Fact]
    public async Task Ejecutar_CuandoEsParaHoy_EntoncesSePermiteAunqueLaHoraHayaPasado()
    {
        // El entre-turno no tiene franja: la hora que mande el formulario no se valida ni se guarda
        var dto = Provisorio() with { HoraInicio = "08:00" };

        var resultado = await Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(dto);

        Assert.Equal(Lunes, resultado.Turno.Fecha);
        Assert.Null(resultado.Turno.HoraInicio);
    }

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoNoExiste_EntoncesLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Provisorio(odontologoId: 999)));
    }
}

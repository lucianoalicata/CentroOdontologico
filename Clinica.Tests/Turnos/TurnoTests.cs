using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

public class TurnoTests
{
    private static readonly DateOnly Fecha = new(2026, 12, 1);

    private static Turno Programado(
        int odontologoId = 1,
        string inicio = "09:00",
        string fin = "09:30",
        DateOnly? fecha = null) =>
        Turno.CrearProgramado(
            odontologoId, 10, fecha ?? Fecha,
            TimeOnly.Parse(inicio), TimeOnly.Parse(fin),
            "Control", OrigenCreacion.TelefonoSecretaria, 99);

    private static Turno EntreTurnoProvisorio() =>
        Turno.CrearEntreTurno(
            1, Fecha, 1, null, "Juan Pérez", "1155554444", null,
            "Dolor de muela", OrigenCreacion.PresencialSecretaria, 99);

    // ---------- Creación de turno programado ----------

    [Fact]
    public void CrearProgramado_CuandoLosDatosSonValidos_EntoncesQuedaPendienteConPaciente()
    {
        var turno = Programado();

        Assert.Equal(TipoTurno.Programado, turno.Tipo);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        Assert.Equal(10, turno.PacienteId);
        Assert.Equal(new TimeOnly(9, 0), turno.HoraInicio);
        Assert.Equal(new TimeOnly(9, 30), turno.HoraFin);
        Assert.Null(turno.OrdenEntreTurno);
        Assert.Equal(OrigenCreacion.TelefonoSecretaria, turno.Origen);
        Assert.Equal(99, turno.CreadoPor);
    }

    [Theory]
    [InlineData("09:30", "09:30")]
    [InlineData("10:00", "09:30")]
    public void CrearProgramado_CuandoLaHoraFinNoEsPosterior_EntoncesLanzaDomainException(string inicio, string fin)
    {
        Assert.Throws<DomainException>(() => Programado(inicio: inicio, fin: fin));
    }

    [Fact]
    public void CrearProgramado_CuandoElPacienteNoEsValido_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Turno.CrearProgramado(
            1, 0, Fecha, new TimeOnly(9, 0), new TimeOnly(9, 30), null, OrigenCreacion.Web, 99));
    }

    [Fact]
    public void CrearProgramado_CuandoElOdontologoNoEsValido_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Programado(odontologoId: 0));
    }

    [Fact]
    public void CrearProgramado_CuandoElMotivoEstaEnBlanco_EntoncesQuedaNulo()
    {
        var turno = Turno.CrearProgramado(
            1, 10, Fecha, new TimeOnly(9, 0), new TimeOnly(9, 30), "   ", OrigenCreacion.Web, 99);

        Assert.Null(turno.Motivo);
    }

    // ---------- Creación de entre-turno ----------

    [Fact]
    public void CrearEntreTurno_CuandoNoHayFicha_EntoncesGuardaDatosProvisorios()
    {
        var turno = Turno.CrearEntreTurno(
            1, Fecha, 2, null, "  Juan Pérez ", " 1155554444 ", "  ",
            null, OrigenCreacion.TelefonoSecretaria, 99);

        Assert.Equal(TipoTurno.EntreTurno, turno.Tipo);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        Assert.Equal(2, turno.OrdenEntreTurno);
        Assert.Null(turno.PacienteId);
        Assert.Equal("Juan Pérez", turno.PacienteWalkInNombre);
        Assert.Equal("1155554444", turno.PacienteWalkInTelefono);
        Assert.Null(turno.PacienteWalkInDni);
        Assert.Null(turno.HoraInicio);
        Assert.Null(turno.HoraFin);
    }

    [Fact]
    public void CrearEntreTurno_CuandoHayFicha_EntoncesVinculaElPaciente()
    {
        var turno = Turno.CrearEntreTurno(
            1, Fecha, 1, 10, null, null, null, null, OrigenCreacion.Odontologo, 99);

        Assert.Equal(10, turno.PacienteId);
        Assert.Null(turno.PacienteWalkInNombre);
    }

    [Theory]
    [InlineData(null, "1155554444")]
    [InlineData("Juan", null)]
    [InlineData("  ", "1155554444")]
    public void CrearEntreTurno_CuandoFaltanNombreOTelefonoSinFicha_EntoncesLanzaDomainException(
        string? nombre, string? telefono)
    {
        Assert.Throws<DomainException>(() => Turno.CrearEntreTurno(
            1, Fecha, 1, null, nombre, telefono, null, null, OrigenCreacion.Web, 99));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CrearEntreTurno_CuandoElOrdenNoEsValido_EntoncesLanzaDomainException(int orden)
    {
        Assert.Throws<DomainException>(() => Turno.CrearEntreTurno(
            1, Fecha, orden, 10, null, null, null, null, OrigenCreacion.Web, 99));
    }

    // ---------- Transiciones ----------

    [Fact]
    public void Iniciar_CuandoEstaPendienteConPaciente_EntoncesPasaAEnCurso()
    {
        var turno = Programado();

        turno.Iniciar();

        Assert.Equal(EstadoTurno.EnCurso, turno.Estado);
    }

    [Fact]
    public void Iniciar_CuandoNoTienePacienteVinculado_EntoncesLanzaDomainException()
    {
        var turno = EntreTurnoProvisorio();

        Assert.Throws<DomainException>(() => turno.Iniciar());
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
    }

    [Fact]
    public void Iniciar_CuandoNoEstaPendiente_EntoncesLanzaDomainException()
    {
        var turno = Programado();
        turno.Iniciar();

        Assert.Throws<DomainException>(() => turno.Iniciar());
    }

    [Fact]
    public void Finalizar_CuandoEstaEnCurso_EntoncesPasaAFinalizado()
    {
        var turno = Programado();
        turno.Iniciar();

        turno.Finalizar();

        Assert.Equal(EstadoTurno.Finalizado, turno.Estado);
    }

    [Fact]
    public void Finalizar_CuandoEstaPendiente_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Programado().Finalizar());
    }

    [Fact]
    public void MarcarAusente_CuandoEstaPendiente_EntoncesPasaAAusente()
    {
        var turno = Programado();

        turno.MarcarAusente();

        Assert.Equal(EstadoTurno.Ausente, turno.Estado);
    }

    [Fact]
    public void MarcarAusente_CuandoEstaEnCurso_EntoncesLanzaDomainException()
    {
        var turno = Programado();
        turno.Iniciar();

        Assert.Throws<DomainException>(() => turno.MarcarAusente());
    }

    [Fact]
    public void Cancelar_CuandoEstaPendiente_EntoncesRegistraMotivoYAutor()
    {
        var turno = Programado();

        turno.Cancelar("  El paciente avisó que no viene  ", 7);

        Assert.Equal(EstadoTurno.Cancelado, turno.Estado);
        Assert.Equal("El paciente avisó que no viene", turno.MotivoCancelacion);
        Assert.Equal(7, turno.CanceladoPor);
        Assert.NotNull(turno.FechaCancelacion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancelar_CuandoNoHayMotivo_EntoncesLanzaDomainException(string? motivo)
    {
        var turno = Programado();

        Assert.Throws<DomainException>(() => turno.Cancelar(motivo!, 7));
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
    }

    [Fact]
    public void Cancelar_CuandoElUsuarioNoEsValido_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Programado().Cancelar("Motivo", 0));
    }

    [Fact]
    public void Cancelar_CuandoYaFueCancelado_EntoncesLanzaDomainException()
    {
        var turno = Programado();
        turno.Cancelar("Motivo", 7);

        Assert.Throws<DomainException>(() => turno.Cancelar("Otro motivo", 7));
    }

    // ---------- Vinculación de paciente ----------

    [Fact]
    public void VincularPaciente_CuandoEsEntreTurnoProvisorio_EntoncesPermiteIniciar()
    {
        var turno = EntreTurnoProvisorio();

        turno.VincularPaciente(25);
        turno.Iniciar();

        Assert.Equal(25, turno.PacienteId);
        Assert.Equal("Juan Pérez", turno.PacienteWalkInNombre);
        Assert.Equal(EstadoTurno.EnCurso, turno.Estado);
    }

    [Fact]
    public void VincularPaciente_CuandoElTurnoEstaCancelado_EntoncesLanzaDomainException()
    {
        var turno = EntreTurnoProvisorio();
        turno.Cancelar("No vino", 7);

        Assert.Throws<DomainException>(() => turno.VincularPaciente(25));
    }

    [Fact]
    public void VincularPaciente_CuandoElPacienteNoEsValido_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => EntreTurnoProvisorio().VincularPaciente(0));
    }

    // ---------- Solapamiento ----------

    [Theory]
    [InlineData("09:00", "09:30", "09:15", "09:45", true)]
    [InlineData("09:00", "10:00", "09:15", "09:45", true)]
    [InlineData("09:00", "09:30", "09:30", "10:00", false)]
    [InlineData("09:00", "09:30", "10:00", "10:30", false)]
    public void SeSolapaCon_CuandoSeComparanFranjas_EntoncesDetectaSuperposicion(
        string inicioA, string finA, string inicioB, string finB, bool esperado)
    {
        var a = Programado(inicio: inicioA, fin: finA);
        var b = Programado(inicio: inicioB, fin: finB);

        Assert.Equal(esperado, a.SeSolapaCon(b));
        Assert.Equal(esperado, b.SeSolapaCon(a));
    }

    [Fact]
    public void SeSolapaCon_CuandoSonDeDistintoOdontologo_EntoncesNoSeSolapan()
    {
        Assert.False(Programado(odontologoId: 1).SeSolapaCon(Programado(odontologoId: 2)));
    }

    [Fact]
    public void SeSolapaCon_CuandoSonDeDistintaFecha_EntoncesNoSeSolapan()
    {
        var otraFecha = Fecha.AddDays(1);

        Assert.False(Programado().SeSolapaCon(Programado(fecha: otraFecha)));
    }

    [Fact]
    public void SeSolapaCon_CuandoUnoEstaCancelado_EntoncesNoSeSolapan()
    {
        var cancelado = Programado();
        cancelado.Cancelar("Motivo", 7);

        Assert.False(Programado().SeSolapaCon(cancelado));
    }

    [Fact]
    public void SeSolapaCon_CuandoUnoEsEntreTurno_EntoncesNoSeSolapan()
    {
        Assert.False(Programado().SeSolapaCon(EntreTurnoProvisorio()));
    }
}

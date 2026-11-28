using Clinica.Aplicacion.Turnos.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

public class DisponibilidadServiceTests
{
    private static readonly DateOnly Lunes = new(2026, 11, 30);

    private readonly FakeHorarios _horarios = new();
    private readonly FakeTurnos _turnos = new();
    private readonly DisponibilidadService _servicio;

    public DisponibilidadServiceTests()
    {
        _servicio = new DisponibilidadService(_horarios, _turnos);
    }

    [Fact]
    public async Task ObtenerFranjasLibres_CuandoHayHorariosYTurnos_EntoncesDevuelveLasLibresEnFormatoHoraMinuto()
    {
        _horarios.Lista.Add(new HorarioDisponible(1, DiaSemana.Lunes, new TimeOnly(9, 0), new TimeOnly(10, 30), 30));
        _turnos.Lista.Add(Turno.CrearProgramado(
            1, 10, Lunes, new TimeOnly(9, 30), new TimeOnly(10, 0), null, OrigenCreacion.Web, 99));

        var libres = await _servicio.ObtenerFranjasLibresAsync(1, Lunes);

        Assert.Equal(2, libres.Count);
        Assert.Equal("09:00", libres[0].HoraInicio);
        Assert.Equal("09:30", libres[0].HoraFin);
        Assert.Equal("10:00", libres[1].HoraInicio);
        Assert.Equal("10:30", libres[1].HoraFin);
    }

    [Fact]
    public async Task ObtenerFranjasLibres_CuandoElOdontologoNoTieneHorarios_EntoncesDevuelveVacioSinConsultarTurnos()
    {
        var libres = await _servicio.ObtenerFranjasLibresAsync(1, Lunes);

        Assert.Empty(libres);
        Assert.False(_turnos.FueConsultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ObtenerFranjasLibres_CuandoElOdontologoNoEsValido_EntoncesLanzaDomainException(int odontologoId)
    {
        await Assert.ThrowsAsync<DomainException>(() => _servicio.ObtenerFranjasLibresAsync(odontologoId, Lunes));
    }

    private class FakeHorarios : IHorarioDisponibleRepository
    {
        public List<HorarioDisponible> Lista { get; } = new();

        public Task<IReadOnlyList<HorarioDisponible>> ListarPorOdontologoAsync(int odontologoId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<HorarioDisponible>>(
                Lista.Where(h => h.OdontologoId == odontologoId && h.Activo).ToList());
    }

    private class FakeTurnos : ITurnoRepository
    {
        public List<Turno> Lista { get; } = new();
        public bool FueConsultado { get; private set; }

        public Task<IReadOnlyList<Turno>> ListarProgramadosActivosAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default)
        {
            FueConsultado = true;
            return Task.FromResult<IReadOnlyList<Turno>>(
                Lista.Where(t => t.OdontologoId == odontologoId && t.Fecha == fecha
                                 && t.Tipo == TipoTurno.Programado && t.EstaActivo).ToList());
        }
    }
}

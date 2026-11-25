using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;

namespace Clinica.Tests.Identidad;

public class PacienteServiceTests
{
    private readonly FakePacientes _pacientes = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly PacienteService _sut;

    public PacienteServiceTests()
    {
        _sut = new PacienteService(_pacientes, _uow);
    }

    [Fact]
    public async Task ObtenerPorId_CuandoNoExiste_EntoncesLanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _sut.ObtenerPorIdAsync(99));
    }

    [Fact]
    public async Task Actualizar_CuandoExiste_EntoncesModificaDatosYGuarda()
    {
        var paciente = ConId(new PacientePerfil("Juan", "Pérez", "30111222", "3811111111", new DateOnly(1990, 5, 10)), 1);
        _pacientes.Lista.Add(paciente);

        var dto = new ActualizarPacienteDto("Juan Carlos", "Pérez", "3812222222", new DateOnly(1990, 5, 10), "Av. Sarmiento 500");
        var actualizado = await _sut.ActualizarAsync(1, dto);

        Assert.Equal("Juan Carlos", actualizado.Nombre);
        Assert.Equal("3812222222", actualizado.Telefono);
        Assert.Equal("Av. Sarmiento 500", actualizado.Domicilio);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task DarDeBaja_CuandoExiste_EntoncesDesactivaYGuarda()
    {
        var paciente = ConId(new PacientePerfil("María", "López", "12345678", "3813333333", new DateOnly(1985, 3, 20)), 2);
        _pacientes.Lista.Add(paciente);

        await _sut.DarDeBajaAsync(2);

        Assert.False(paciente.Activo);
        Assert.NotNull(paciente.FechaBaja);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Reactivar_CuandoEstaDadoDeBaja_EntoncesReactivaYGuarda()
    {
        var paciente = ConId(new PacientePerfil("María", "López", "12345678", "3813333333", new DateOnly(1985, 3, 20)), 2);
        paciente.Desactivar();
        _pacientes.Lista.Add(paciente);

        var reactivado = await _sut.ReactivarAsync(2);

        Assert.True(reactivado.Activo);
        Assert.Null(paciente.FechaBaja);
        Assert.Equal(1, _uow.Guardados);
    }

    private static T ConId<T>(T entidad, int id) where T : Entidad
    {
        typeof(Entidad).GetProperty(nameof(Entidad.Id))!.SetValue(entidad, id);
        return entidad;
    }

    private class FakePacientes : IPacienteRepository
    {
        public List<PacientePerfil> Lista { get; } = new();

        public Task AgregarAsync(PacientePerfil paciente, CancellationToken ct = default)
        {
            Lista.Add(paciente);
            return Task.CompletedTask;
        }

        public Task<PacientePerfil?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Id == id && p.Activo));

        public Task<PacientePerfil?> ObtenerPorIdIncluyendoInactivosAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Id == id));

        public Task<PacientePerfil?> ObtenerPorDniIncluyendoInactivosAsync(string dni, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Dni == dni));

        public Task<ResultadoPaginado<PacientePerfil>> BuscarPaginadoAsync(string? filtro, int pageIndex, int pageSize, CancellationToken ct = default) =>
            throw new NotImplementedException();
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int Guardados { get; private set; }
        public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        {
            Guardados++;
            return Task.FromResult(1);
        }
    }
}
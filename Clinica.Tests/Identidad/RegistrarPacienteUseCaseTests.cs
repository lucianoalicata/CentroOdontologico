using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;

namespace Clinica.Tests.Identidad;

public class RegistrarPacienteUseCaseTests
{
    private readonly FakePacientes _pacientes = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly RegistrarPacienteUseCase _sut;

    public RegistrarPacienteUseCaseTests()
    {
        _sut = new RegistrarPacienteUseCase(_pacientes, _uow);
    }

    [Fact]
    public async Task Ejecutar_CuandoEsNuevo_EntoncesLoAgregaYGuarda()
    {
        var dto = new RegistrarPacienteDto("Carlos", "Ruiz", "35.123.456", "3815551111", new DateOnly(1992, 8, 10), "San Martín 450");

        var resultado = await _sut.EjecutarAsync(dto);

        Assert.Equal("35123456", resultado.Dni);
        Assert.Equal("San Martín 450", resultado.Domicilio);
        Assert.True(resultado.Activo);
        Assert.Single(_pacientes.Lista);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoDniYaExisteActivo_EntoncesLanzaDomainException()
    {
        _pacientes.Lista.Add(new PacientePerfil("Ana", "Gómez", "30111222", "3814440000", new DateOnly(1990, 1, 1)));
        var dto = new RegistrarPacienteDto("Otra", "Persona", "30111222", "3819999999", new DateOnly(1995, 5, 5), null);

        await Assert.ThrowsAsync<DomainException>(() => _sut.EjecutarAsync(dto));
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoDniExisteDadoDeBaja_EntoncesLoReactivaYActualizaSinDuplicar()
    {
        var inactivo = new PacientePerfil("Ana", "Vieja", "30111222", "3810000000", new DateOnly(1990, 1, 1), "Dir vieja");
        inactivo.Desactivar();
        _pacientes.Lista.Add(inactivo);

        var dto = new RegistrarPacienteDto("Ana", "Nueva", "30111222", "3812223333", new DateOnly(1990, 1, 1), "Av. Aconquija 1200");

        var resultado = await _sut.EjecutarAsync(dto);

        Assert.Single(_pacientes.Lista);
        Assert.True(resultado.Activo);
        Assert.Equal("Nueva", resultado.Apellido);
        Assert.Equal("Av. Aconquija 1200", resultado.Domicilio);
        Assert.Equal(1, _uow.Guardados);
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
            Task.FromResult(Lista.FirstOrDefault(p => p.Id == id));

        public Task<PacientePerfil?> ObtenerPorDniIncluyendoInactivosAsync(string dni, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Dni == dni));

        public Task<PacientePerfil?> ObtenerPorIdIncluyendoInactivosAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Id == id));

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
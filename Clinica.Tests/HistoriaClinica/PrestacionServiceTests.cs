using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Aplicacion.HistoriaClinica.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;
using Clinica.Infraestructura.HistoriaClinica.Datos;

namespace Clinica.Tests.HistoriaClinica;

public class PrestacionServiceTests
{
    private readonly FakePrestaciones _prestaciones = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly PrestacionService _sut;

    public PrestacionServiceTests()
    {
        _sut = new PrestacionService(_prestaciones, _uow);
    }

    private static CrearPrestacionDto NuevaDto(string codigo = "01.01") =>
        new(codigo, "Consulta", CategoriaPrestacion.Consulta, 30, 15000m, false);

    [Fact]
    public async Task Crear_CuandoElCodigoNoExiste_EntoncesLaAgregaActivaYGuarda()
    {
        var creada = await _sut.CrearAsync(NuevaDto());

        Assert.Single(_prestaciones.Lista);
        Assert.Equal("01.01", creada.Codigo);
        Assert.True(creada.Activo);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Crear_CuandoYaExisteUnaActivaConElMismoCodigo_EntoncesLanzaDomainException()
    {
        _prestaciones.Agregar(new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m), 1);

        await Assert.ThrowsAsync<DomainException>(() => _sut.CrearAsync(NuevaDto(" 01.01 ")));
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Crear_CuandoElCodigoEsDeUnaDadaDeBaja_EntoncesLaReactivaConLosDatosNuevos()
    {
        var anterior = new Prestacion("01.01", "Consulta vieja", CategoriaPrestacion.Otra, 15, 1000m);
        anterior.Desactivar();
        _prestaciones.Agregar(anterior, 1);

        var dto = new CrearPrestacionDto("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m, true);
        var resultado = await _sut.CrearAsync(dto);

        Assert.Single(_prestaciones.Lista);
        Assert.Equal(1, resultado.Id);
        Assert.True(resultado.Activo);
        Assert.Equal("Consulta", resultado.Titulo);
        Assert.Equal(15000m, resultado.PrecioParticular);
        Assert.True(resultado.RequiereAutorizacionPrevia);
    }

    [Fact]
    public async Task Crear_CuandoLosDatosSonInvalidos_EntoncesLanzaDomainExceptionYNoGuarda()
    {
        var dto = new CrearPrestacionDto("01.01", "Consulta", CategoriaPrestacion.Consulta, 0, 15000m, false);

        await Assert.ThrowsAsync<DomainException>(() => _sut.CrearAsync(dto));
        Assert.Empty(_prestaciones.Lista);
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Actualizar_CuandoExiste_EntoncesModificaYGuarda()
    {
        _prestaciones.Agregar(new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m), 1);

        var dto = new ActualizarPrestacionDto("Consulta completa", CategoriaPrestacion.Consulta, 40, 18000m, false);
        var actualizada = await _sut.ActualizarAsync(1, dto);

        Assert.Equal("Consulta completa", actualizada.Titulo);
        Assert.Equal(40, actualizada.DuracionEstimadaMinutos);
        Assert.Equal(18000m, actualizada.PrecioParticular);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Actualizar_CuandoNoExiste_EntoncesLanzaNoEncontradoException()
    {
        var dto = new ActualizarPrestacionDto("Consulta", CategoriaPrestacion.Consulta, 30, 15000m, false);

        await Assert.ThrowsAsync<NoEncontradoException>(() => _sut.ActualizarAsync(99, dto));
    }

    [Fact]
    public async Task Actualizar_CuandoEstaDadaDeBaja_EntoncesLanzaNoEncontradoException()
    {
        var prestacion = new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m);
        prestacion.Desactivar();
        _prestaciones.Agregar(prestacion, 1);

        var dto = new ActualizarPrestacionDto("Consulta", CategoriaPrestacion.Consulta, 30, 15000m, false);

        await Assert.ThrowsAsync<NoEncontradoException>(() => _sut.ActualizarAsync(1, dto));
    }

    [Fact]
    public async Task DarDeBaja_CuandoExiste_EntoncesLaDesactivaYGuarda()
    {
        var prestacion = new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m);
        _prestaciones.Agregar(prestacion, 1);

        await _sut.DarDeBajaAsync(1);

        Assert.False(prestacion.Activo);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task DarDeBaja_CuandoNoExiste_EntoncesLanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _sut.DarDeBajaAsync(5));
    }

    [Fact]
    public async Task Reactivar_CuandoEstaDadaDeBaja_EntoncesQuedaActivaYGuarda()
    {
        var prestacion = new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m);
        prestacion.Desactivar();
        _prestaciones.Agregar(prestacion, 1);

        var resultado = await _sut.ReactivarAsync(1);

        Assert.True(resultado.Activo);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task ListarActivas_CuandoHayDadasDeBaja_EntoncesSoloDevuelveLasActivas()
    {
        var baja = new Prestacion("02.01", "Obturación", CategoriaPrestacion.Operatoria, 45, 28000m);
        baja.Desactivar();
        _prestaciones.Agregar(new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m), 1);
        _prestaciones.Agregar(baja, 2);

        var activas = await _sut.ListarActivasAsync(null, null);

        Assert.Single(activas);
        Assert.Equal("01.01", activas.First().Codigo);
    }

    [Fact]
    public async Task ListarTodas_CuandoHayDadasDeBaja_EntoncesTambienLasDevuelve()
    {
        var baja = new Prestacion("02.01", "Obturación", CategoriaPrestacion.Operatoria, 45, 28000m);
        baja.Desactivar();
        _prestaciones.Agregar(new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m), 1);
        _prestaciones.Agregar(baja, 2);

        var todas = await _sut.ListarTodasAsync(null, null);

        Assert.Equal(2, todas.Count);
    }

    [Fact]
    public async Task ListarActivas_CuandoHayFiltroYCategoria_EntoncesDevuelveSoloLasQueCoinciden()
    {
        _prestaciones.Agregar(new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m), 1);
        _prestaciones.Agregar(new Prestacion("02.01", "Obturación simple", CategoriaPrestacion.Operatoria, 45, 28000m), 2);
        _prestaciones.Agregar(new Prestacion("02.08", "Restauración con resina", CategoriaPrestacion.Operatoria, 60, 35000m), 3);

        var resultado = await _sut.ListarActivasAsync("obtur", CategoriaPrestacion.Operatoria);

        Assert.Single(resultado);
        Assert.Equal("02.01", resultado.First().Codigo);
    }

    [Fact]
    public async Task ObtenerPorId_CuandoNoExiste_EntoncesLanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => _sut.ObtenerPorIdAsync(1));
    }

    [Fact]
    public void Nomenclador_CuandoSeGeneraElDeEjemplo_EntoncesLosCodigosSonUnicosYValidos()
    {
        var prestaciones = NomencladorSeeder.Prestaciones();

        Assert.NotEmpty(prestaciones);
        Assert.Equal(prestaciones.Count, prestaciones.Select(p => p.Codigo).Distinct().Count());
        Assert.Contains(prestaciones, p => p.RequiereAutorizacionPrevia);
    }

    private class FakePrestaciones : IPrestacionRepository
    {
        public List<Prestacion> Lista { get; } = new();

        public void Agregar(Prestacion prestacion, int id)
        {
            typeof(Entidad).GetProperty(nameof(Entidad.Id))!.SetValue(prestacion, id);
            Lista.Add(prestacion);
        }

        public Task AgregarAsync(Prestacion prestacion, CancellationToken ct = default)
        {
            Lista.Add(prestacion);
            return Task.CompletedTask;
        }

        public Task<Prestacion?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Id == id && p.Activo));

        public Task<Prestacion?> ObtenerPorIdIncluyendoInactivasAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Id == id));

        public Task<Prestacion?> ObtenerPorCodigoIncluyendoInactivasAsync(string codigo, CancellationToken ct = default) =>
            Task.FromResult(Lista.FirstOrDefault(p => p.Codigo == codigo.Trim()));

        public Task<IReadOnlyList<Prestacion>> ListarAsync(
            string? filtro, CategoriaPrestacion? categoria, bool incluirInactivas, CancellationToken ct = default)
        {
            IEnumerable<Prestacion> query = Lista;
            if (!incluirInactivas) query = query.Where(p => p.Activo);
            if (categoria.HasValue) query = query.Where(p => p.Categoria == categoria.Value);
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var t = filtro.Trim().ToLower();
                query = query.Where(p => p.Codigo.ToLower().Contains(t) || p.Titulo.ToLower().Contains(t));
            }
            return Task.FromResult<IReadOnlyList<Prestacion>>(query.OrderBy(p => p.Codigo).ToList());
        }
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

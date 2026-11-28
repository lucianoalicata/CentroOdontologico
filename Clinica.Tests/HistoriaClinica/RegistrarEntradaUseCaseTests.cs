using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Aplicacion.HistoriaClinica.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;
using Clinica.Tests.Turnos;

namespace Clinica.Tests.HistoriaClinica;

public class RegistrarEntradaUseCaseTests
{
    private const int Paciente = 10;
    private const int Odontologo = 7;

    private readonly FakeHistorias _historias = new();
    private readonly FakeConsultaPacientes _pacientes = new();
    private readonly FakeCatalogoPrestaciones _catalogo = new();
    private readonly FakeUnitOfWork _uow = new();

    public RegistrarEntradaUseCaseTests()
    {
        _pacientes.Activos.Add(Paciente);
        _catalogo.Agregar(new Prestacion("01.01", "Consulta", CategoriaPrestacion.Consulta, 30, 15000m));
    }

    private RegistrarEntradaUseCase Crear(FakeUsuarioActual usuario) =>
        new(_historias, _pacientes, _catalogo, usuario, _uow);

    private static RegistrarEntradaDto Dto(
        string? comentario = "Control de rutina",
        IReadOnlyList<RegistrarPrestacionDto>? prestaciones = null,
        int? corrige = null) =>
        new(Paciente, comentario, corrige, prestaciones);

    // ---------- Caso de exito ----------

    [Fact]
    public async Task Ejecutar_CuandoElOdontologoRegistraConPrestaciones_EntoncesCreaLaHistoriaYCopiaLosDatosDelCatalogo()
    {
        var dto = Dto(prestaciones: new[] { new RegistrarPrestacionDto(1, Cantidad: 2, PiezaDental: 16, MesesGarantia: 12) });

        var resultado = await Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(dto);

        Assert.Equal(Odontologo, resultado.OdontologoId);
        Assert.Equal("Control de rutina", resultado.Comentario);
        var prestacion = Assert.Single(resultado.Prestaciones);
        Assert.Equal("01.01", prestacion.Codigo);
        Assert.Equal("Consulta", prestacion.Titulo);
        Assert.Equal(15000m, prestacion.PrecioUnitario);
        Assert.Equal(2, prestacion.Cantidad);
        Assert.Equal(30000m, prestacion.Subtotal);
        Assert.Equal(16, prestacion.PiezaDental);
        Assert.Equal(12, prestacion.MesesGarantia);

        var historia = Assert.Single(_historias.Lista);
        Assert.Equal(Paciente, historia.PacienteId);
        Assert.Single(historia.Entradas);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoLaHistoriaYaExiste_EntoncesAgregaLaEntradaSinCrearOtraHistoria()
    {
        var historia = _historias.Sembrar(Paciente);
        _historias.SembrarEntrada(historia, Odontologo);

        await Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(Dto());

        Assert.Single(_historias.Lista);
        Assert.Equal(2, historia.Entradas.Count);
        Assert.Equal(1, _uow.Guardados);
    }

    // ---------- Permisos ----------

    [Fact]
    public async Task Ejecutar_CuandoEsSecretaria_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Secretaria()).EjecutarAsync(Dto()));
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoEsAdministrador_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Administrador()).EjecutarAsync(Dto()));
    }

    [Fact]
    public async Task Ejecutar_CuandoEsPaciente_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Crear(FakeUsuarioActual.Paciente(Paciente)).EjecutarAsync(Dto()));
    }

    // ---------- No encontrado ----------

    [Fact]
    public async Task Ejecutar_CuandoElPacienteNoExiste_EntoncesLanzaNoEncontrado()
    {
        var dto = new RegistrarEntradaDto(999, "Control", null, null);

        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(dto));
        Assert.Empty(_historias.Lista);
    }

    [Fact]
    public async Task Ejecutar_CuandoLaPrestacionNoExisteEnElCatalogo_EntoncesLanzaNoEncontradoYNoGuarda()
    {
        var dto = Dto(prestaciones: new[] { new RegistrarPrestacionDto(999) });

        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(dto));
        Assert.Empty(_historias.Lista);
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoLaPrestacionEstaDadaDeBaja_EntoncesLanzaNoEncontrado()
    {
        var dadaDeBaja = _catalogo.Agregar(new Prestacion("09.09", "Vieja", CategoriaPrestacion.Otra, 15, 1000m));
        dadaDeBaja.Desactivar();
        var dto = Dto(prestaciones: new[] { new RegistrarPrestacionDto(dadaDeBaja.Id) });

        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(dto));
    }

    // ---------- Reglas del dominio ----------

    [Fact]
    public async Task Ejecutar_CuandoNoHayComentarioNiPrestaciones_EntoncesLanzaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(Dto(comentario: "   ")));
        Assert.Empty(_historias.Lista);
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoLaCantidadEsCero_EntoncesLanzaDomainException()
    {
        var dto = Dto(prestaciones: new[] { new RegistrarPrestacionDto(1, Cantidad: 0) });

        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(dto));
    }

    // ---------- Correcciones ----------

    [Fact]
    public async Task Ejecutar_CuandoCorrigeUnaEntradaDeLaMismaHistoria_EntoncesGuardaLaReferencia()
    {
        var historia = _historias.Sembrar(Paciente);
        var original = _historias.SembrarEntrada(historia, Odontologo, "Pieza equivocada");

        var resultado = await Crear(FakeUsuarioActual.Odontologo(Odontologo))
            .EjecutarAsync(Dto("Corrijo la pieza", corrige: original.Id));

        Assert.Equal(original.Id, resultado.CorrigeEntradaId);
        Assert.Equal(2, historia.Entradas.Count);
    }

    [Fact]
    public async Task Ejecutar_CuandoCorrigeUnaEntradaInexistente_EntoncesLanzaDomainException()
    {
        _historias.Sembrar(Paciente);

        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(Dto(corrige: 99)));
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Ejecutar_CuandoCorrigeSinTenerHistoria_EntoncesLanzaDomainExceptionYNoLaCrea()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            Crear(FakeUsuarioActual.Odontologo(Odontologo)).EjecutarAsync(Dto(corrige: 1)));
        Assert.Empty(_historias.Lista);
    }
}
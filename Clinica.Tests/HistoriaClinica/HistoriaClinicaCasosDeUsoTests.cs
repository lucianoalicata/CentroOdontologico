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

public class HistoriaClinicaCasosDeUsoTests
{
    private const int Paciente = 10;
    private const int Odontologo = 7;

    private readonly FakeHistorias _historias = new();
    private readonly FakeConsultaPacientes _pacientes = new();
    private readonly FakeUnitOfWork _uow = new();

    public HistoriaClinicaCasosDeUsoTests()
    {
        _pacientes.Activos.Add(Paciente);
    }

    private ObtenerHistoriaClinicaUseCase Obtener(FakeUsuarioActual u) => new(_historias, _pacientes, u, _uow);
    private ListarEvolucionUseCase Evolucion(FakeUsuarioActual u) => new(_historias, _pacientes, u);
    private ListarHistorialPrestacionesUseCase Historial(FakeUsuarioActual u) => new(_historias, _pacientes, u);
    private ActualizarAlertasMedicasUseCase Alertas(FakeUsuarioActual u) => new(_historias, _pacientes, u, _uow);

    private static FakeUsuarioActual Odonto() => FakeUsuarioActual.Odontologo(Odontologo);

    // ---------- Obtener o crear ----------

    [Fact]
    public async Task Obtener_CuandoNoTieneHistoria_EntoncesLaCreaYGuarda()
    {
        var resultado = await Obtener(Odonto()).EjecutarAsync(Paciente);

        Assert.Equal(Paciente, resultado.PacienteId);
        Assert.Null(resultado.AlertasMedicas);
        Assert.Single(_historias.Lista);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task Obtener_CuandoYaTieneHistoria_EntoncesDevuelveLaExistenteSinGuardar()
    {
        var existente = _historias.Sembrar(Paciente);
        existente.ActualizarAlertasMedicas("Alergia a la penicilina");

        var resultado = await Obtener(Odonto()).EjecutarAsync(Paciente);

        Assert.Equal(existente.Id, resultado.Id);
        Assert.Equal("Alergia a la penicilina", resultado.AlertasMedicas);
        Assert.Single(_historias.Lista);
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task Obtener_CuandoEsAdministrador_EntoncesPuedeConsultar()
    {
        var resultado = await Obtener(FakeUsuarioActual.Administrador()).EjecutarAsync(Paciente);

        Assert.Equal(Paciente, resultado.PacienteId);
    }

    [Fact]
    public async Task Obtener_CuandoEsSecretaria_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Obtener(FakeUsuarioActual.Secretaria()).EjecutarAsync(Paciente));
        Assert.Empty(_historias.Lista);
    }

    [Fact]
    public async Task Obtener_CuandoElPacienteNoExiste_EntoncesLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => Obtener(Odonto()).EjecutarAsync(999));
    }

    // ---------- Evolución ----------

    [Fact]
    public async Task ListarEvolucion_CuandoNoHayHistoria_EntoncesDevuelveListaVaciaYNoLaCrea()
    {
        var lista = await Evolucion(Odonto()).EjecutarAsync(Paciente);

        Assert.Empty(lista);
        Assert.Empty(_historias.Lista);
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task ListarEvolucion_CuandoHayEntradas_EntoncesLasDevuelveEnOrdenYMarcaLaCorregida()
    {
        var historia = _historias.Sembrar(Paciente);
        var original = _historias.SembrarEntrada(historia, Odontologo, "Obturación en pieza equivocada");
        var correccion = _historias.SembrarEntrada(historia, Odontologo, "Corrijo la pieza", corrigeEntradaId: original.Id);

        var lista = await Evolucion(Odonto()).EjecutarAsync(Paciente);

        Assert.Equal(2, lista.Count);
        Assert.Equal(original.Id, lista[0].Id);
        Assert.True(lista[0].FueCorregida);
        Assert.Equal(correccion.Id, lista[1].Id);
        Assert.False(lista[1].FueCorregida);
        Assert.Equal(original.Id, lista[1].CorrigeEntradaId);
    }

    [Fact]
    public async Task ListarEvolucion_CuandoEsPaciente_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Evolucion(FakeUsuarioActual.Paciente(Paciente)).EjecutarAsync(Paciente));
    }

    // ---------- Historial de prestaciones ----------

    [Fact]
    public async Task ListarHistorialPrestaciones_CuandoHayEntradas_EntoncesAplanaLasPrestacionesEnOrden()
    {
        var historia = _historias.Sembrar(Paciente);
        var primera = _historias.SembrarEntrada(historia, Odontologo, "Primera",
            new[] { new PrestacionRealizada("01.01", "Consulta", 15000m) });
        _historias.SembrarEntrada(historia, Odontologo, "Segunda",
            new[]
            {
                new PrestacionRealizada("02.01", "Obturación", 40000m, 2, 16, 12),
                new PrestacionRealizada("02.02", "Limpieza", 20000m)
            });

        var lista = await Historial(Odonto()).EjecutarAsync(Paciente);

        Assert.Equal(3, lista.Count);
        Assert.Equal(new[] { "01.01", "02.01", "02.02" }, lista.Select(p => p.Codigo).ToArray());
        Assert.Equal(primera.Id, lista[0].EntradaId);
        Assert.Equal(80000m, lista[1].Subtotal);
        Assert.Equal(16, lista[1].PiezaDental);
        Assert.All(lista, p => Assert.False(p.EntradaCorregida));
    }

    [Fact]
    public async Task ListarHistorialPrestaciones_CuandoNoHayHistoria_EntoncesDevuelveListaVacia()
    {
        var lista = await Historial(Odonto()).EjecutarAsync(Paciente);

        Assert.Empty(lista);
    }

    // ---------- Alertas médicas ----------

    [Fact]
    public async Task ActualizarAlertas_CuandoEsOdontologo_EntoncesGuardaLasAlertas()
    {
        var historia = _historias.Sembrar(Paciente);

        var resultado = await Alertas(Odonto())
            .EjecutarAsync(Paciente, new ActualizarAlertasDto("  Alergia al látex  "));

        Assert.Equal("Alergia al látex", resultado.AlertasMedicas);
        Assert.Equal("Alergia al látex", historia.AlertasMedicas);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task ActualizarAlertas_CuandoNoHayHistoria_EntoncesLaCreaConLasAlertas()
    {
        await Alertas(Odonto()).EjecutarAsync(Paciente, new ActualizarAlertasDto("Hipertensión"));

        var historia = Assert.Single(_historias.Lista);
        Assert.Equal("Hipertensión", historia.AlertasMedicas);
        Assert.Equal(1, _uow.Guardados);
    }

    [Fact]
    public async Task ActualizarAlertas_CuandoSeEnviaTextoVacio_EntoncesQuitaLasAlertas()
    {
        var historia = _historias.Sembrar(Paciente);
        historia.ActualizarAlertasMedicas("Alergia");

        var resultado = await Alertas(Odonto()).EjecutarAsync(Paciente, new ActualizarAlertasDto("  "));

        Assert.Null(resultado.AlertasMedicas);
    }

    [Fact]
    public async Task ActualizarAlertas_CuandoSuperaElMaximo_EntoncesLanzaDomainException()
    {
        _historias.Sembrar(Paciente);
        var largo = new string('a', HistoriaClinicaPaciente.LongitudMaximaAlertas + 1);

        await Assert.ThrowsAsync<DomainException>(() =>
            Alertas(Odonto()).EjecutarAsync(Paciente, new ActualizarAlertasDto(largo)));
        Assert.Equal(0, _uow.Guardados);
    }

    [Fact]
    public async Task ActualizarAlertas_CuandoEsAdministrador_EntoncesLanzaAccesoDenegado()
    {
        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Alertas(FakeUsuarioActual.Administrador()).EjecutarAsync(Paciente, new ActualizarAlertasDto("x")));
    }
}
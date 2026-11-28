using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.HistoriaClinica.Puertos;
using Clinica.Dominio.HistoriaClinica;
using Clinica.Tests.Turnos;

namespace Clinica.Tests.HistoriaClinica;

// Implementaciones falsas compartidas por las pruebas de los casos de uso de la historia clínica.

internal class FakeHistorias : IHistoriaClinicaRepository
{
    private int _siguienteId = 1;
    private int _siguienteEntradaId = 1;

    public List<HistoriaClinicaPaciente> Lista { get; } = new();

    /// <summary>Agrega una historia ya existente (como si estuviera guardada), asignándole Id.</summary>
    public HistoriaClinicaPaciente Sembrar(int pacienteId)
    {
        var historia = new HistoriaClinicaPaciente(pacienteId);
        Lista.Add(AsignadorId.ConId(historia, _siguienteId++));
        return historia;
    }

    /// <summary>Agrega una entrada ya existente en la historia, asignándole Id.</summary>
    public EntradaHistoriaClinica SembrarEntrada(
        HistoriaClinicaPaciente historia,
        int odontologoId,
        string? comentario = "Control",
        IEnumerable<PrestacionRealizada>? prestaciones = null,
        int? corrigeEntradaId = null)
    {
        var entrada = historia.AgregarEntrada(odontologoId, comentario, prestaciones, corrigeEntradaId);
        AsignadorId.ConId(entrada, _siguienteEntradaId++);
        return entrada;
    }

    public Task AgregarAsync(HistoriaClinicaPaciente historia, CancellationToken ct = default)
    {
        Lista.Add(AsignadorId.ConId(historia, _siguienteId++));
        return Task.CompletedTask;
    }

    public Task<HistoriaClinicaPaciente?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(h => h.Id == id));

    public Task<HistoriaClinicaPaciente?> ObtenerPorPacienteIdAsync(int pacienteId, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(h => h.PacienteId == pacienteId));

    public Task<HistoriaClinicaPaciente?> ObtenerPorPacienteIdConEntradasAsync(int pacienteId, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(h => h.PacienteId == pacienteId));

    public Task<EntradaHistoriaClinica?> ObtenerEntradaPorIdAsync(int entradaId, CancellationToken ct = default) =>
        Task.FromResult(Lista.SelectMany(h => h.Entradas).FirstOrDefault(e => e.Id == entradaId));

    public Task<IReadOnlyList<EntradaHistoriaClinica>> ListarEntradasAsync(int historiaClinicaId, CancellationToken ct = default)
    {
        var historia = Lista.FirstOrDefault(h => h.Id == historiaClinicaId);
        IReadOnlyList<EntradaHistoriaClinica> entradas = historia is null
            ? new List<EntradaHistoriaClinica>()
            : historia.Entradas.OrderBy(e => e.Id).ToList();
        return Task.FromResult(entradas);
    }
}

internal class FakeConsultaPacientes : IConsultaPacientes
{
    public HashSet<int> Activos { get; } = new();

    public Task<bool> ExistePacienteActivoAsync(int pacienteId, CancellationToken ct = default) =>
        Task.FromResult(Activos.Contains(pacienteId));
}

internal class FakeCatalogoPrestaciones : IPrestacionRepository
{
    private int _siguienteId = 1;

    public List<Prestacion> Lista { get; } = new();

    /// <summary>Agrega una prestación al catálogo, asignándole Id.</summary>
    public Prestacion Agregar(Prestacion prestacion)
    {
        Lista.Add(AsignadorId.ConId(prestacion, _siguienteId++));
        return prestacion;
    }

    public Task AgregarAsync(Prestacion prestacion, CancellationToken ct = default)
    {
        Agregar(prestacion);
        return Task.CompletedTask;
    }

    public Task<Prestacion?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(p => p.Id == id && p.Activo));

    public Task<Prestacion?> ObtenerPorIdIncluyendoInactivasAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(p => p.Id == id));

    public Task<Prestacion?> ObtenerPorCodigoIncluyendoInactivasAsync(string codigo, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(p => p.Codigo == codigo));

    public Task<IReadOnlyList<Prestacion>> ListarAsync(
        string? filtro, CategoriaPrestacion? categoria, bool incluirInactivas, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Prestacion>>(
            Lista.Where(p => incluirInactivas || p.Activo).ToList());
}

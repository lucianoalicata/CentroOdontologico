using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Aplicacion.HistoriaClinica.Servicios;

/// <summary>
/// Catálogo de prestaciones. El alta, la edición y la baja son del administrador;
/// el listado de activas es para todo el personal (la API aplica las políticas).
/// </summary>
public interface IPrestacionService
{
    Task<IReadOnlyList<PrestacionDto>> ListarActivasAsync(string? filtro, CategoriaPrestacion? categoria, CancellationToken ct = default);
    Task<IReadOnlyList<PrestacionDto>> ListarTodasAsync(string? filtro, CategoriaPrestacion? categoria, CancellationToken ct = default);
    Task<PrestacionDto> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<PrestacionDto> CrearAsync(CrearPrestacionDto dto, CancellationToken ct = default);
    Task<PrestacionDto> ActualizarAsync(int id, ActualizarPrestacionDto dto, CancellationToken ct = default);
    Task DarDeBajaAsync(int id, CancellationToken ct = default);
    Task<PrestacionDto> ReactivarAsync(int id, CancellationToken ct = default);
}

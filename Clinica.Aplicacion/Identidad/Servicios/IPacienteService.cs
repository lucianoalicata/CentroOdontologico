using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Dominio.Comun;

namespace Clinica.Aplicacion.Identidad.Servicios;

public interface IPacienteService
{
    Task<PacienteDto> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<ResultadoPaginado<PacienteDto>> BuscarPaginadoAsync(string? filtro, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken ct = default);
    Task DarDeBajaAsync(int id, CancellationToken ct = default);
    Task<PacienteDto> ReactivarAsync(int id, CancellationToken ct = default);
}
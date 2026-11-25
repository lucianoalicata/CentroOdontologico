using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Identidad;

public interface IPacienteRepository
{
    Task AgregarAsync(PacientePerfil paciente, CancellationToken ct = default);
    Task<PacientePerfil?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<PacientePerfil?> ObtenerPorDniIncluyendoInactivosAsync(string dni, CancellationToken ct = default);

    Task<ResultadoPaginado<PacientePerfil>> BuscarPaginadoAsync(
        string? filtro,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);
}
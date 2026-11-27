namespace Clinica.Dominio.HistoriaClinica;

public interface IPrestacionRepository
{
    Task AgregarAsync(Prestacion prestacion, CancellationToken ct = default);
    Task<Prestacion?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Prestacion?> ObtenerPorIdIncluyendoInactivasAsync(int id, CancellationToken ct = default);
    Task<Prestacion?> ObtenerPorCodigoIncluyendoInactivasAsync(string codigo, CancellationToken ct = default);

    /// <summary>
    /// Lista ordenada por código. El filtro busca por código o título.
    /// Con <paramref name="incluirInactivas"/> también devuelve las dadas de baja.
    /// </summary>
    Task<IReadOnlyList<Prestacion>> ListarAsync(
        string? filtro,
        CategoriaPrestacion? categoria,
        bool incluirInactivas,
        CancellationToken ct = default);
}

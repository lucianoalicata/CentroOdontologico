namespace Clinica.Dominio.HistoriaClinica;

public interface IArchivoAdjuntoRepository
{
    Task AgregarAsync(ArchivoAdjunto adjunto, CancellationToken ct = default);

    Task<ArchivoAdjunto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Adjuntos de una entrada (solo lectura), del más antiguo al más reciente.</summary>
    Task<IReadOnlyList<ArchivoAdjunto>> ListarPorEntradaAsync(int entradaId, CancellationToken ct = default);
}

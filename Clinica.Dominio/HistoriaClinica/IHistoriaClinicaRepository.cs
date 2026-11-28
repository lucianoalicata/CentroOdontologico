namespace Clinica.Dominio.HistoriaClinica;

public interface IHistoriaClinicaRepository
{
    Task AgregarAsync(HistoriaClinicaPaciente historia, CancellationToken ct = default);

    /// <summary>Historia clínica sin sus entradas (rastreada).</summary>
    Task<HistoriaClinicaPaciente?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Historia clínica del paciente, sin sus entradas (rastreada).</summary>
    Task<HistoriaClinicaPaciente?> ObtenerPorPacienteIdAsync(int pacienteId, CancellationToken ct = default);

    /// <summary>
    /// Historia clínica del paciente con todas sus entradas y prestaciones realizadas (rastreada).
    /// Hace falta para agregar una entrada, porque la historia valida que la entrada a corregir le pertenezca.
    /// </summary>
    Task<HistoriaClinicaPaciente?> ObtenerPorPacienteIdConEntradasAsync(int pacienteId, CancellationToken ct = default);

    /// <summary>Una entrada con sus prestaciones realizadas (solo lectura).</summary>
    Task<EntradaHistoriaClinica?> ObtenerEntradaPorIdAsync(int entradaId, CancellationToken ct = default);

    /// <summary>
    /// Entradas de una historia con sus prestaciones realizadas (solo lectura),
    /// en orden cronológico: de la más antigua a la más reciente.
    /// </summary>
    Task<IReadOnlyList<EntradaHistoriaClinica>> ListarEntradasAsync(int historiaClinicaId, CancellationToken ct = default);
}

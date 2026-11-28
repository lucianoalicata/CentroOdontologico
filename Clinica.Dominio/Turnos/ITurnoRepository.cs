namespace Clinica.Dominio.Turnos;

public interface ITurnoRepository
{
    Task AgregarAsync(Turno turno, CancellationToken ct = default);

    Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Turnos programados del odontólogo en la fecha que siguen activos (pendientes o en curso).</summary>
    Task<IReadOnlyList<Turno>> ListarProgramadosActivosAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default);

    /// <summary>Todos los turnos del odontólogo en la fecha, en cualquier estado (para armar la agenda).</summary>
    Task<IReadOnlyList<Turno>> ListarPorOdontologoYFechaAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default);

    /// <summary>
    /// Mayor OrdenEntreTurno del odontólogo en la fecha, incluidos los cancelados para no repetir números.
    /// Devuelve 0 si todavía no hay entre-turnos ese día.
    /// </summary>
    Task<int> ObtenerUltimoOrdenEntreTurnoAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default);
}

namespace Clinica.Dominio.Turnos;

public interface IHorarioDisponibleRepository
{
    Task AgregarAsync(HorarioDisponible horario, CancellationToken ct = default);

    /// <summary>Horario activo por Id, o null si no existe o fue dado de baja.</summary>
    Task<HorarioDisponible?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Horarios activos del odontólogo, de todos los días, ordenados por día y hora de inicio.</summary>
    Task<IReadOnlyList<HorarioDisponible>> ListarPorOdontologoAsync(int odontologoId, CancellationToken ct = default);
}

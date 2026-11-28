namespace Clinica.Dominio.Turnos;

/// <summary>Contrato de persistencia de horarios. La implementación se agrega en S1-14.</summary>
public interface IHorarioDisponibleRepository
{
    /// <summary>Horarios activos del odontólogo, de todos los días de la semana.</summary>
    Task<IReadOnlyList<HorarioDisponible>> ListarPorOdontologoAsync(int odontologoId, CancellationToken ct = default);
}

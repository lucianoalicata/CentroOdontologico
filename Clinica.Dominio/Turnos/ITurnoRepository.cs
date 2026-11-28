namespace Clinica.Dominio.Turnos;

/// <summary>Contrato de persistencia de turnos. La implementación se agrega en S1-14.</summary>
public interface ITurnoRepository
{
    /// <summary>Turnos programados del odontólogo en la fecha que siguen activos (pendientes o en curso).</summary>
    Task<IReadOnlyList<Turno>> ListarProgramadosActivosAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default);
}

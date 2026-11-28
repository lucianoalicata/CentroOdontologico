using Clinica.Aplicacion.Turnos.Dtos;

namespace Clinica.Aplicacion.Turnos.Servicios;

public interface IDisponibilidadService
{
    /// <summary>Franjas libres del odontólogo en la fecha, ordenadas por hora.</summary>
    Task<IReadOnlyList<FranjaLibreDto>> ObtenerFranjasLibresAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default);
}

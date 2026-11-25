using Clinica.Aplicacion.Identidad.Dtos;

namespace Clinica.Aplicacion.Identidad.Puertos;

public interface IOdontologoConsulta
{
    Task<IReadOnlyList<OdontologoActivoDto>> ListarActivosAsync(CancellationToken ct = default);
}
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Puertos;

namespace Clinica.Aplicacion.Identidad.Servicios;

public class ListarOdontologosActivosUseCase
{
    private readonly IOdontologoConsulta _consulta;

    public ListarOdontologosActivosUseCase(IOdontologoConsulta consulta)
    {
        _consulta = consulta;
    }

    public Task<IReadOnlyList<OdontologoActivoDto>> EjecutarAsync(CancellationToken ct = default) =>
        _consulta.ListarActivosAsync(ct);
}
using Clinica.Api.Seguridad;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("api/odontologos")]
[Authorize(Policy = Politicas.SoloPersonal)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class OdontologosController : ControllerBase
{
    private readonly ListarOdontologosActivosUseCase _listarActivos;

    public OdontologosController(ListarOdontologosActivosUseCase listarActivos)
    {
        _listarActivos = listarActivos;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OdontologoActivoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OdontologoActivoDto>>> ListarActivos(CancellationToken ct) =>
        Ok(await _listarActivos.EjecutarAsync(ct));
}
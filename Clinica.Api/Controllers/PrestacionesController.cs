using Clinica.Api.Seguridad;
using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Aplicacion.HistoriaClinica.Servicios;
using Clinica.Dominio.HistoriaClinica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// Catálogo de prestaciones. El personal consulta las activas; el administrador
/// gestiona el catálogo completo (alta, edición, baja y reactivación).
/// </summary>
[ApiController]
[Route("api/prestaciones")]
[Authorize(Policy = Politicas.SoloPersonal)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class PrestacionesController : ControllerBase
{
    private readonly IPrestacionService _prestaciones;

    public PrestacionesController(IPrestacionService prestaciones)
    {
        _prestaciones = prestaciones;
    }

    /// <summary>Prestaciones activas, para elegir al registrar una atención.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PrestacionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PrestacionDto>>> ListarActivas(
        [FromQuery] string? filtro,
        [FromQuery] CategoriaPrestacion? categoria,
        CancellationToken ct) =>
        Ok(await _prestaciones.ListarActivasAsync(filtro, categoria, ct));

    /// <summary>Catálogo completo, incluidas las dadas de baja (solo administrador).</summary>
    [HttpGet("todas")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    [ProducesResponseType<IReadOnlyList<PrestacionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PrestacionDto>>> ListarTodas(
        [FromQuery] string? filtro,
        [FromQuery] CategoriaPrestacion? categoria,
        CancellationToken ct) =>
        Ok(await _prestaciones.ListarTodasAsync(filtro, categoria, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<PrestacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrestacionDto>> ObtenerPorId(int id, CancellationToken ct) =>
        Ok(await _prestaciones.ObtenerPorIdAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    [ProducesResponseType<PrestacionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrestacionDto>> Crear(CrearPrestacionDto dto, CancellationToken ct)
    {
        var creada = await _prestaciones.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    [ProducesResponseType<PrestacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrestacionDto>> Actualizar(int id, ActualizarPrestacionDto dto, CancellationToken ct) =>
        Ok(await _prestaciones.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(int id, CancellationToken ct)
    {
        await _prestaciones.DarDeBajaAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    [Authorize(Policy = Politicas.SoloAdministrador)]
    [ProducesResponseType<PrestacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrestacionDto>> Reactivar(int id, CancellationToken ct) =>
        Ok(await _prestaciones.ReactivarAsync(id, ct));
}

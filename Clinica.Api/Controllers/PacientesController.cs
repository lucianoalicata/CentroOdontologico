using Clinica.Api.Seguridad;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Servicios;
using Clinica.Dominio.Comun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("api/pacientes")]
[Authorize(Policy = Politicas.SoloPersonal)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacientes;
    private readonly RegistrarPacienteUseCase _registrarPaciente;

    public PacientesController(
        IPacienteService pacientes,
        RegistrarPacienteUseCase registrarPaciente)
    {
        _pacientes = pacientes;
        _registrarPaciente = registrarPaciente;
    }

    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<PacienteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<PacienteDto>>> Buscar(
        [FromQuery] string? filtro,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default) =>
        Ok(await _pacientes.BuscarPaginadoAsync(filtro, pageIndex, pageSize, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<PacienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PacienteDto>> ObtenerPorId(int id, CancellationToken ct) =>
        Ok(await _pacientes.ObtenerPorIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<PacienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PacienteDto>> Registrar(RegistrarPacienteDto dto, CancellationToken ct)
    {
        var creado = await _registrarPaciente.EjecutarAsync(dto, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<PacienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PacienteDto>> Actualizar(int id, ActualizarPacienteDto dto, CancellationToken ct) =>
        Ok(await _pacientes.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(int id, CancellationToken ct)
    {
        await _pacientes.DarDeBajaAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/reactivar")]
    [ProducesResponseType<PacienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PacienteDto>> Reactivar(int id, CancellationToken ct) =>
        Ok(await _pacientes.ReactivarAsync(id, ct));
}
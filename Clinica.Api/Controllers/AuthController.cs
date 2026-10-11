using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

// Controller fino: recibe, delega en AuthService y devuelve. Sin lógica de negocio.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IUsuarioActual _usuarioActual;

    public AuthController(IAuthService auth, IUsuarioActual usuarioActual)
    {
        _auth = auth;
        _usuarioActual = usuarioActual;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken ct) =>
        Ok(await _auth.LoginAsync(dto, ct));

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UsuarioSesionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioSesionDto>> Me(CancellationToken ct) =>
        Ok(await _auth.ObtenerSesionAsync(_usuarioActual.UsuarioId, ct));
}
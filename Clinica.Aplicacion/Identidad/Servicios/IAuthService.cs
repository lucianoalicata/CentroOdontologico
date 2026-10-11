using Clinica.Aplicacion.Identidad.Dtos;

namespace Clinica.Aplicacion.Identidad.Servicios;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<UsuarioSesionDto> ObtenerSesionAsync(int usuarioId, CancellationToken ct = default);
}
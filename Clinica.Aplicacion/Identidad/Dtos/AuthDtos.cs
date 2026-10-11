namespace Clinica.Aplicacion.Identidad.Dtos;

public record LoginDto(string Email, string Password);

// Lo que el frontend necesita saber de quien inició sesión.
// Los ids de perfil son nulos si el usuario no tiene ese perfil.
public record UsuarioSesionDto(
    int Id,
    string Nombre,
    string Apellido,
    string Email,
    IReadOnlyList<string> Roles,
    int? PacienteId,
    int? OdontologoId,
    int? SecretariaId);

public record AuthResponseDto(string Token, DateTime ExpiraEn, UsuarioSesionDto Usuario);
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Excepciones;
using Clinica.Aplicacion.Identidad.Puertos;
using Clinica.Dominio.Identidad;

namespace Clinica.Aplicacion.Identidad.Servicios;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPerfilRepository _perfiles;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwt;

    public AuthService(
        IUsuarioRepository usuarios,
        IPerfilRepository perfiles,
        IPasswordHasher hasher,
        IJwtProvider jwt)
    {
        _usuarios = usuarios;
        _perfiles = perfiles;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new CredencialesInvalidasException();

        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObtenerPorEmailAsync(email, ct);

        // Un único error para email inexistente, contraseña incorrecta y usuario de baja:
        // así no revelamos qué cuentas existen.
        if (usuario is null || !usuario.Activo || !_hasher.Verificar(dto.Password, usuario.PasswordHash))
            throw new CredencialesInvalidasException();

        var sesion = await ConstruirSesionAsync(usuario, ct);
        var token = _jwt.Generar(sesion);

        return new AuthResponseDto(token.Token, token.ExpiraEn, sesion);
    }

    public async Task<UsuarioSesionDto> ObtenerSesionAsync(int usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct);

        // Token todavía vigente de un usuario que ya no existe o fue dado de baja
        if (usuario is null || !usuario.Activo)
            throw new CredencialesInvalidasException("La sesión ya no es válida.");

        return await ConstruirSesionAsync(usuario, ct);
    }

    // Solo busca el perfil correspondiente a los roles que el usuario realmente tiene.
    private async Task<UsuarioSesionDto> ConstruirSesionAsync(Usuario usuario, CancellationToken ct)
    {
        var paciente = usuario.TieneRol(RolUsuario.Paciente)
            ? await _perfiles.ObtenerPacientePorUsuarioIdAsync(usuario.Id, ct) : null;
        var odontologo = usuario.TieneRol(RolUsuario.Odontologo)
            ? await _perfiles.ObtenerOdontologoPorUsuarioIdAsync(usuario.Id, ct) : null;
        var secretaria = usuario.TieneRol(RolUsuario.Secretaria)
            ? await _perfiles.ObtenerSecretariaPorUsuarioIdAsync(usuario.Id, ct) : null;

        return new UsuarioSesionDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email,
            usuario.Roles.Select(r => r.Rol.ToString()).ToList(),
            paciente?.Id,
            odontologo?.Id,
            secretaria?.Id);
    }
}
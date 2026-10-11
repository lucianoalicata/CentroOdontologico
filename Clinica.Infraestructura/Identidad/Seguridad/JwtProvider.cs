using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Puertos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinica.Infraestructura.Identidad.Seguridad;

public class JwtProvider : IJwtProvider
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _credenciales;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtProvider(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        _credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);
    }

    public TokenGenerado Generar(UsuarioSesionDto sesion)
    {
        var expiraUtc = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, sesion.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, sesion.Email),
            new(ClaimNombres.Nombre, sesion.Nombre),
            new(ClaimNombres.Apellido, sesion.Apellido),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Un claim por cada rol: el token los serializa como lista
        claims.AddRange(sesion.Roles.Select(rol => new Claim(ClaimNombres.Rol, rol)));

        AgregarSiTiene(claims, ClaimNombres.PacienteId, sesion.PacienteId);
        AgregarSiTiene(claims, ClaimNombres.OdontologoId, sesion.OdontologoId);
        AgregarSiTiene(claims, ClaimNombres.SecretariaId, sesion.SecretariaId);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraUtc,
            signingCredentials: _credenciales);

        return new TokenGenerado(_handler.WriteToken(token), expiraUtc);
    }

    private static void AgregarSiTiene(List<Claim> claims, string nombre, int? valor)
    {
        if (valor.HasValue)
            claims.Add(new Claim(nombre, valor.Value.ToString()));
    }
}
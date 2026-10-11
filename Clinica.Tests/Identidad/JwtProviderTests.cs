using System.IdentityModel.Tokens.Jwt;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Infraestructura.Identidad.Seguridad;
using Microsoft.Extensions.Options;

namespace Clinica.Tests.Identidad;

public class JwtProviderTests
{
    private readonly JwtProvider _sut = new(Options.Create(new JwtSettings
    {
        SecretKey = new string('k', 40),
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpirationMinutes = 60
    }));

    private static UsuarioSesionDto Sesion(int? paciente = null, int? odontologo = null, int? secretaria = null) =>
        new(7, "Ana", "Pérez", "ana@clinica.com",
            new[] { "Secretaria", "Administrador" }, paciente, odontologo, secretaria);

    private static JwtSecurityToken Leer(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void Generar_IncluyeIdEmailYTodosLosRoles()
    {
        var jwt = Leer(_sut.Generar(Sesion()).Token);

        Assert.Equal("7", jwt.Subject);
        Assert.Equal("ana@clinica.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);

        var roles = jwt.Claims.Where(c => c.Type == ClaimNombres.Rol).Select(c => c.Value).ToList();
        Assert.Equal(2, roles.Count);
        Assert.Contains("Secretaria", roles);
        Assert.Contains("Administrador", roles);
    }

    [Fact]
    public void Generar_SoloIncluyeLosIdsDePerfilQueExisten()
    {
        var jwt = Leer(_sut.Generar(Sesion(secretaria: 3)).Token);

        Assert.Equal("3", jwt.Claims.First(c => c.Type == ClaimNombres.SecretariaId).Value);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimNombres.PacienteId);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimNombres.OdontologoId);
    }

    [Fact]
    public void Generar_UsaEmisorYAudienciaConfigurados()
    {
        var jwt = Leer(_sut.Generar(Sesion()).Token);

        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Equal("test-audience", jwt.Audiences.Single());
    }

    [Fact]
    public void Generar_LaExpiracionDelTokenCoincideConLaDevuelta()
    {
        var resultado = _sut.Generar(Sesion());
        var jwt = Leer(resultado.Token);

        Assert.True(Math.Abs((resultado.ExpiraEn - jwt.ValidTo).TotalSeconds) < 2);
        Assert.True(resultado.ExpiraEn > DateTime.UtcNow.AddMinutes(59));
    }
}
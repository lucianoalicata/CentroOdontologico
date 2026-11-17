using System.Security.Claims;
using Clinica.Api.Seguridad;
using Clinica.Aplicacion.Identidad;
using Clinica.Dominio.Identidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Tests.Api;

public class PoliticasDeAutorizacionTests
{
    private readonly IAuthorizationService _autorizacion;

    public PoliticasDeAutorizacionTests()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddAuthorization(PoliticasDeAutorizacion.Configurar);
        _autorizacion = servicios.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal UsuarioCon(params RolUsuario[] roles)
    {
        var claims = roles.Select(rol => new Claim(ClaimNombres.Rol, rol.ToString()));

        var identidad = new ClaimsIdentity(
            claims,
            authenticationType: "Bearer",
            nameType: ClaimNombres.UsuarioId,
            roleType: ClaimNombres.Rol);

        return new ClaimsPrincipal(identidad);
    }

    private async Task<bool> Cumple(ClaimsPrincipal usuario, string politica) =>
        (await _autorizacion.AuthorizeAsync(usuario, politica)).Succeeded;

    [Fact]
    public async Task SoloPersonal_CuandoElUsuarioEsSecretaria_EntoncesCumple()
    {
        var secretaria = UsuarioCon(RolUsuario.Secretaria);

        Assert.True(await Cumple(secretaria, Politicas.SoloPersonal));
    }

    [Theory]
    // Política: SoloPersonal (Secretaria, Odontologo, Administrador -> true; Paciente -> false)
    [InlineData(RolUsuario.Secretaria, Politicas.SoloPersonal, true)]
    [InlineData(RolUsuario.Odontologo, Politicas.SoloPersonal, true)]
    [InlineData(RolUsuario.Administrador, Politicas.SoloPersonal, true)]
    [InlineData(RolUsuario.Paciente, Politicas.SoloPersonal, false)]
    // Política: SoloOdontologo (solo Odontologo -> true)
    [InlineData(RolUsuario.Secretaria, Politicas.SoloOdontologo, false)]
    [InlineData(RolUsuario.Odontologo, Politicas.SoloOdontologo, true)]
    [InlineData(RolUsuario.Administrador, Politicas.SoloOdontologo, false)]
    [InlineData(RolUsuario.Paciente, Politicas.SoloOdontologo, false)]
    // Política: SoloAdministrador (solo Administrador -> true)
    [InlineData(RolUsuario.Secretaria, Politicas.SoloAdministrador, false)]
    [InlineData(RolUsuario.Odontologo, Politicas.SoloAdministrador, false)]
    [InlineData(RolUsuario.Administrador, Politicas.SoloAdministrador, true)]
    [InlineData(RolUsuario.Paciente, Politicas.SoloAdministrador, false)]
    public async Task EvaluarPolitica_ParaCadaRol_DevuelveResultadoEsperado(
        RolUsuario rol, string politica, bool esperado)
    {
        var usuario = UsuarioCon(rol);

        var resultado = await Cumple(usuario, politica);

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(Politicas.SoloPersonal)]
    [InlineData(Politicas.SoloOdontologo)]
    [InlineData(Politicas.SoloAdministrador)]
    public async Task EvaluarPolitica_CuandoUsuarioAutenticadoSinRoles_NoCumpleNinguna(string politica)
    {
        var usuarioSinRoles = UsuarioCon();

        Assert.False(await Cumple(usuarioSinRoles, politica));
    }

    [Fact]
    public async Task SoloAdministrador_CuandoUsuarioTieneSecretariaYAdministrador_EntoncesCumple()
    {
        var usuarioMultirrol = UsuarioCon(RolUsuario.Secretaria, RolUsuario.Administrador);

        Assert.True(await Cumple(usuarioMultirrol, Politicas.SoloAdministrador));
    }

    [Theory]
    [InlineData(Politicas.SoloPersonal)]
    [InlineData(Politicas.SoloOdontologo)]
    [InlineData(Politicas.SoloAdministrador)]
    public async Task EvaluarPolitica_CuandoTieneClaimDeRolPeroNoEstaAutenticado_NoCumpleNinguna(string politica)
    {
        var claims = new[] { new Claim(ClaimNombres.Rol, nameof(RolUsuario.Administrador)) };
        var identidadNoAutenticada = new ClaimsIdentity(
            claims,
            authenticationType: null,
            nameType: ClaimNombres.UsuarioId,
            roleType: ClaimNombres.Rol);
        var principal = new ClaimsPrincipal(identidadNoAutenticada);

        Assert.False(await Cumple(principal, politica));
    }
}
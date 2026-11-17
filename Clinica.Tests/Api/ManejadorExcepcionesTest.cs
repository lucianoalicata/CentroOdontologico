using System.Text.Json;
using Clinica.Api.Middleware;
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Identidad.Excepciones;
using Clinica.Dominio.Comun;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinica.Tests.Api;

public class ManejadorExcepcionesTests
{
    private static async Task<(int Status, ProblemDetails Cuerpo)> Manejar(Exception excepcion)
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        var manejador = new ManejadorExcepciones(NullLogger<ManejadorExcepciones>.Instance);

        var manejada = await manejador.TryHandleAsync(contexto, excepcion, CancellationToken.None);

        Assert.True(manejada);

        contexto.Response.Body.Position = 0;
        var cuerpo = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            contexto.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return (contexto.Response.StatusCode, cuerpo!);
    }

    [Fact]
    public async Task TryHandleAsync_CuandoEsDomainException_EntoncesResponde400ConElMensaje()
    {
        var (status, cuerpo) = await Manejar(new DomainException("El DNI es obligatorio."));

        Assert.Equal(400, status);
        Assert.Equal("El DNI es obligatorio.", cuerpo.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_CuandoSonCredencialesInvalidas_EntoncesResponde401()
    {
        var (status, _) = await Manejar(new CredencialesInvalidasException());

        Assert.Equal(401, status);
    }

    [Fact]
    public async Task TryHandleAsync_CuandoLaExcepcionEsDesconocida_EntoncesResponde500SinRevelarElMensaje()
    {
        var (status, cuerpo) = await Manejar(new InvalidOperationException("clave-interna-secreta"));

        Assert.Equal(500, status);
        Assert.DoesNotContain("clave-interna-secreta", cuerpo.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_CuandoNoSeEncuentraElRecurso_EntoncesResponde404ConElMensaje()
    {
        var (status, cuerpo) = await Manejar(new NoEncontradoException("recurso"));
        Assert.Equal(404, status);
        Assert.Equal("No se encontró ${recurso}.", cuerpo.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_CuandoElAccesoEstaDenegado_EntoncesResponde403ConElMensaje()
    {
        var (status, cuerpo) = await Manejar(new AccesoDenegadoException());
        Assert.Equal(403, status);
        Assert.Equal("No tiene permisos para realizar esta acción.", cuerpo.Detail);
    }
}
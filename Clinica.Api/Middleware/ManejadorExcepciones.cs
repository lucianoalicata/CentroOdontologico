using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Identidad.Excepciones;
using Clinica.Dominio.Comun;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Middleware;

public class ManejadorExcepciones : IExceptionHandler
{
    private readonly ILogger<ManejadorExcepciones> _logger;

    public ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            CredencialesInvalidasException => (StatusCodes.Status401Unauthorized, "No autenticado"),
            DomainException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            AccesoDenegadoException => (StatusCodes.Status403Forbidden, "No autorizado"),
            NoEncontradoException => (StatusCodes.Status404NotFound, "No encontrado"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", http.Request.Method, http.Request.Path);

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado."
                : ex.Message
        };

        http.Response.StatusCode = status;
        await http.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
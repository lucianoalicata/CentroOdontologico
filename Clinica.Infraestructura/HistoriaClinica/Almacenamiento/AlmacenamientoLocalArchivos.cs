using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.HistoriaClinica.Puertos;
using Microsoft.Extensions.Options;

namespace Clinica.Infraestructura.HistoriaClinica.Almacenamiento;

/// <summary>
/// Guarda los adjuntos en una carpeta del disco. Solo para desarrollo: el almacenamiento
/// definitivo se decide más adelante. La clave es un GUID más la extensión original.
/// </summary>
public class AlmacenamientoLocalArchivos : IAlmacenamientoArchivos
{
    private const int TamanoBuffer = 81920;

    private readonly string _rutaBase;

    public AlmacenamientoLocalArchivos(IOptions<AlmacenamientoSettings> opciones)
    {
        _rutaBase = Path.GetFullPath(opciones.Value.RutaBase);
    }

    public async Task<string> GuardarAsync(Stream contenido, string nombreOriginal, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_rutaBase);

        var clave = Guid.NewGuid().ToString("N") + ExtensionSegura(nombreOriginal);
        var ruta = ResolverRuta(clave)!;   // GUID + extensión saneada: siempre queda dentro de la carpeta base

        try
        {
            await using var destino = new FileStream(
                ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None, TamanoBuffer, useAsync: true);
            await contenido.CopyToAsync(destino, ct);
        }
        catch
        {
            // Si la copia falla a mitad, no queda un archivo parcial en disco.
            if (File.Exists(ruta)) File.Delete(ruta);
            throw;
        }

        return clave;
    }

    public Task<Stream> AbrirLecturaAsync(string clave, CancellationToken ct = default)
    {
        var ruta = ResolverRuta(clave);
        if (ruta is null || !File.Exists(ruta))
            throw new NoEncontradoException("el archivo adjunto");

        try
        {
            Stream stream = new FileStream(
                ruta, FileMode.Open, FileAccess.Read, FileShare.Read, TamanoBuffer, useAsync: true);
            return Task.FromResult(stream);
        }
        catch (FileNotFoundException)
        {
            throw new NoEncontradoException("el archivo adjunto");
        }
    }

    public Task EliminarAsync(string clave, CancellationToken ct = default)
    {
        var ruta = ResolverRuta(clave);
        if (ruta is not null && File.Exists(ruta))
            File.Delete(ruta);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Devuelve la ruta completa de la clave, o null si no es un nombre de archivo simple
    /// dentro de la carpeta base (evita que una clave con "..", separadores o una unidad
    /// de disco apunte afuera).
    /// </summary>
    private string? ResolverRuta(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave != Path.GetFileName(clave))
            return null;

        try
        {
            var ruta = Path.GetFullPath(Path.Combine(_rutaBase, clave));
            var baseConSeparador = _rutaBase.EndsWith(Path.DirectorySeparatorChar)
                ? _rutaBase
                : _rutaBase + Path.DirectorySeparatorChar;

            return ruta.StartsWith(baseConSeparador, StringComparison.OrdinalIgnoreCase) ? ruta : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    // Solo se conserva la extensión si es corta y alfanumérica; si no, el archivo queda sin extensión.
    private static string ExtensionSegura(string? nombreOriginal)
    {
        var extension = Path.GetExtension(nombreOriginal ?? "").ToLowerInvariant();
        var valida = extension.Length is > 1 and <= 11 && extension.Skip(1).All(char.IsAsciiLetterOrDigit);
        return valida ? extension : "";
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Dominio.Comun;

namespace Clinica.Dominio.HistoriaClinica;

/// <summary>
/// Archivo (radiografía, estudio) vinculado a una entrada de la historia clínica.
/// Guarda solo los metadatos: el contenido vive detrás de IAlmacenamientoArchivos.
/// Es un dato clínico: no se borra.
/// </summary>
public class ArchivoAdjunto : Entidad
{
    public const int LongitudMaximaNombre = 255;
    public const int LongitudMaximaTipoContenido = 100;
    public const int LongitudMaximaClave = 500;
    public const long TamanoMaximoBytes = 10L * 1024 * 1024;

    private static readonly Dictionary<string, (string TipoContenido, TipoAdjunto Tipo)> Formatos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ("image/jpeg", TipoAdjunto.Imagen),
            [".jpeg"] = ("image/jpeg", TipoAdjunto.Imagen),
            [".png"] = ("image/png", TipoAdjunto.Imagen),
            [".pdf"] = ("application/pdf", TipoAdjunto.Documento),
        };

    /// <summary>Referencia a EntradaHistoriaClinica.Id (solo por Id).</summary>
    public int EntradaHistoriaClinicaId { get; private set; }
    public string NombreOriginal { get; private set; } = null!;
    public string TipoContenido { get; private set; } = null!;
    public TipoAdjunto Tipo { get; private set; }
    public long TamanoBytes { get; private set; }

    /// <summary>Identificador opaco que devuelve IAlmacenamientoArchivos al guardar.</summary>
    public string ClaveAlmacenamiento { get; private set; } = null!;

    /// <summary>Referencia a Usuario.Id de quien subió el archivo.</summary>
    public int SubidoPorUsuarioId { get; private set; }

    protected ArchivoAdjunto() { }

    public ArchivoAdjunto(
        int entradaHistoriaClinicaId,
        string nombreOriginal,
        string tipoContenido,
        long tamanoBytes,
        string claveAlmacenamiento,
        int subidoPorUsuarioId)
    {
        EntradaHistoriaClinicaId = Guard.IdValido(entradaHistoriaClinicaId, "Entrada de historia clínica");
        Tipo = Validar(nombreOriginal, tipoContenido, tamanoBytes);
        NombreOriginal = NormalizarNombre(nombreOriginal);
        TipoContenido = tipoContenido.Trim().ToLowerInvariant();
        TamanoBytes = tamanoBytes;
        ClaveAlmacenamiento = ValidarClave(claveAlmacenamiento);
        SubidoPorUsuarioId = Guard.IdValido(subidoPorUsuarioId, "Usuario");
    }

    /// <summary>
    /// Valida formato, tipo de contenido y tamaño. Es pública para que el caso de uso
    /// pueda rechazar el archivo antes de guardarlo en el almacenamiento.
    /// </summary>
    public static TipoAdjunto Validar(string? nombreOriginal, string? tipoContenido, long tamanoBytes)
    {
        var nombre = NormalizarNombre(nombreOriginal);

        if (!Formatos.TryGetValue(Path.GetExtension(nombre), out var formato))
            throw new DomainException("El formato del archivo no está permitido. Se aceptan JPG, PNG y PDF.");

        var contenido = Guard.NoVacio(tipoContenido, "Tipo de contenido");
        if (!string.Equals(contenido, formato.TipoContenido, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("El tipo de contenido no coincide con la extensión del archivo.");

        if (tamanoBytes <= 0)
            throw new DomainException("El archivo está vacío.");

        if (tamanoBytes > TamanoMaximoBytes)
            throw new DomainException($"El archivo no puede superar los {TamanoMaximoBytes / (1024 * 1024)} MB.");

        return formato.Tipo;
    }

    private static string NormalizarNombre(string? nombreOriginal)
    {
        // Se descarta cualquier ruta que venga en el nombre (/ o \).
        var limpio = Guard.NoVacio(nombreOriginal, "Nombre del archivo").Replace('\\', '/');
        var nombre = Path.GetFileName(limpio);

        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del archivo no es válido.");
        if (nombre.Length > LongitudMaximaNombre)
            throw new DomainException($"El nombre del archivo no puede superar los {LongitudMaximaNombre} caracteres.");

        return nombre;
    }

    private static string ValidarClave(string clave)
    {
        var limpia = Guard.NoVacio(clave, "Clave de almacenamiento");
        if (limpia.Length > LongitudMaximaClave)
            throw new DomainException($"La clave de almacenamiento no puede superar los {LongitudMaximaClave} caracteres.");
        return limpia;
    }
}
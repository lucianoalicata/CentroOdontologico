using System;
using System.Collections.Generic;
using System.Text;

namespace Clinica.Aplicacion.HistoriaClinica.Puertos;

/// <summary>
/// Almacenamiento del contenido de los archivos adjuntos. Hoy se implementa en disco
/// (S1-29); el almacenamiento definitivo se decide más adelante.
/// </summary>
public interface IAlmacenamientoArchivos
{
    /// <summary>Guarda el contenido y devuelve la clave opaca con la que se lo recupera.</summary>
    Task<string> GuardarAsync(Stream contenido, string nombreOriginal, CancellationToken ct = default);

    /// <summary>Abre el contenido para lectura. Lanza NoEncontradoException si la clave no existe.</summary>
    Task<Stream> AbrirLecturaAsync(string clave, CancellationToken ct = default);

    /// <summary>
    /// Elimina el contenido. Es solo para compensar cuando falla el guardado en base de datos
    /// después de haber escrito el archivo; los adjuntos ya registrados no se borran.
    /// </summary>
    Task EliminarAsync(string clave, CancellationToken ct = default);
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Clinica.Infraestructura.HistoriaClinica.Almacenamiento;

// Se llena desde la sección "Almacenamiento" de la configuración.
public class AlmacenamientoSettings
{
    public const string Seccion = "Almacenamiento";

    // Carpeta donde se guardan los adjuntos. Si es relativa, Program.cs la resuelve
    // contra la carpeta de la API.
    public string RutaBase { get; set; } = "";
}

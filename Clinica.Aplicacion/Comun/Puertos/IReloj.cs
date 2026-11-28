namespace Clinica.Aplicacion.Comun.Puertos;

/// <summary>
/// Hora actual. Se inyecta para poder fijarla en las pruebas.
/// </summary>
public interface IReloj
{
    /// <summary>
    /// Fecha y hora actual en la zona horaria de la clínica. Se usa para comparar con
    /// la fecha y hora de un turno, que se cargan en hora local.
    /// </summary>
    DateTime AhoraEnClinica { get; }
}

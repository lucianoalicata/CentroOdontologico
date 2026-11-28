using System.Globalization;
using Clinica.Aplicacion.Turnos.Dtos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Aplicacion.Turnos;

/// <summary>Conversiones compartidas por los casos de uso de Turnos.</summary>
public static class TurnoMapeo
{
    public static TurnoDto ADto(Turno t) => new(
        t.Id,
        t.Tipo,
        t.Estado,
        t.OdontologoId,
        t.PacienteId,
        t.Fecha,
        t.HoraInicio.HasValue ? AHoraTexto(t.HoraInicio.Value) : null,
        t.HoraFin.HasValue ? AHoraTexto(t.HoraFin.Value) : null,
        t.OrdenEntreTurno,
        t.PacienteWalkInNombre,
        t.PacienteWalkInTelefono,
        t.PacienteWalkInDni,
        t.Motivo,
        t.Origen,
        t.MotivoCancelacion);

    /// <summary>Formato fijo "HH:mm", independiente de la configuración regional del servidor.</summary>
    public static string AHoraTexto(TimeOnly hora) =>
        hora.ToString("HH\\:mm", CultureInfo.InvariantCulture);

    /// <summary>Interpreta una hora "HH:mm" o lanza DomainException con un mensaje legible.</summary>
    public static TimeOnly LeerHora(string? texto, string campo)
    {
        if (TimeOnly.TryParseExact(texto?.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
            return hora;

        throw new DomainException($"{campo} no es válida. Usá el formato HH:mm.");
    }
}

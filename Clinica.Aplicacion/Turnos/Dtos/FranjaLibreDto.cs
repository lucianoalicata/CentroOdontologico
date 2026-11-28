namespace Clinica.Aplicacion.Turnos.Dtos;

/// <summary>Franja libre para programar un turno. Las horas viajan como texto "HH:mm", como las espera el frontend.</summary>
public record FranjaLibreDto(string HoraInicio, string HoraFin);

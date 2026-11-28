using Clinica.Dominio.Turnos;

namespace Clinica.Aplicacion.Turnos.Dtos;

/// <summary>
/// Datos para programar un turno. Las horas viajan como "HH:mm" y la fecha como "YYYY-MM-DD".
/// El origen solo lo elige el personal (teléfono o presencial); para pacientes y odontólogos se deduce del rol.
/// </summary>
public record ProgramarTurnoDto(
    int OdontologoId,
    int PacienteId,
    DateOnly Fecha,
    string HoraInicio,
    string? Motivo,
    OrigenCreacion? Origen = null);

public record TurnoDto(
    int Id,
    TipoTurno Tipo,
    EstadoTurno Estado,
    int OdontologoId,
    int? PacienteId,
    DateOnly Fecha,
    string? HoraInicio,
    string? HoraFin,
    int? OrdenEntreTurno,
    string? PacienteWalkInNombre,
    string? PacienteWalkInTelefono,
    string? PacienteWalkInDni,
    string? Motivo,
    OrigenCreacion Origen,
    string? MotivoCancelacion);

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

/// <summary>
/// Datos para crear un entre-turno. Si el paciente ya tiene ficha se informa PacienteId;
/// si no, nombre y teléfono (y el DNI si lo tiene) como datos provisorios.
/// HoraInicio se acepta por compatibilidad con el formulario pero no se guarda:
/// el entre-turno no ocupa franja y se atiende por orden de llegada.
/// </summary>
public record CrearEntreTurnoDto(
    int OdontologoId,
    DateOnly Fecha,
    int? PacienteId,
    string? Nombre,
    string? Telefono,
    string? Dni,
    string? Motivo,
    string? HoraInicio = null,
    OrigenCreacion? Origen = null);

/// <summary>Ficha existente que coincide con el DNI informado, para ofrecer vincularla.</summary>
public record PacienteSugeridoDto(int Id, string Nombre, string Apellido, string Dni);

public record EntreTurnoCreadoDto(TurnoDto Turno, PacienteSugeridoDto? PacienteSugerido);

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

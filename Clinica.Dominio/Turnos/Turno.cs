using Clinica.Dominio.Turnos;

namespace Clinica.Dominio;

public class Turno
{
    public int Id { get; private set; }
    public string PacienteWalkInNombre { get; private set; } = string.Empty;
    public string PacienteWalkInTelefono { get; private set; } = string.Empty;

    public int OdontologoId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TimeOnly? HoraInicio { get; private set; }
    public TimeOnly? HoraFin { get; private set; }

    public int? OrdenEntreTurno { get; private set; }
    public EstadoTurno Estado { get; private set; }
    public TipoTurno Tipo { get; private set; }
    public OrigenCreacion Origen { get; private set; }

    public int CreadoPor { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public string? Notas { get; private set; }

    // Constructor sin parámetros para Entity Framework Core
    protected Turno() { }

    // Método de fábrica para Turno Programado
    public static Turno CrearProgramado(
        string pacienteNombre,
        string pacienteTelefono,
        int odontologoId,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        OrigenCreacion origen,
        int creadoPor,
        string? notas = null)
    {
        ValidarDatosComunes(pacienteNombre, odontologoId, creadoPor);

        if (horaFin <= horaInicio)
            throw new InvalidOperationException("La hora de fin debe ser posterior a la hora de inicio.");

        return new Turno
        {
            PacienteWalkInNombre = pacienteNombre.Trim(),
            PacienteWalkInTelefono = pacienteTelefono?.Trim() ?? string.Empty,
            OdontologoId = odontologoId,
            Fecha = fecha,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Tipo = TipoTurno.Programado,
            OrdenEntreTurno = null,
            Estado = EstadoTurno.Pendiente,
            Origen = origen,
            CreadoPor = creadoPor,
            FechaCreacion = DateTime.UtcNow,
            Notas = notas
        };
    }

    // Método de fábrica para Entre-Turno
    public static Turno CrearEntreTurno(
        string pacienteNombre,
        string pacienteTelefono,
        int odontologoId,
        DateOnly fecha,
        int ordenCalculado,
        OrigenCreacion origen,
        int creadoPor,
        string? notas = null)
    {
        ValidarDatosComunes(pacienteNombre, odontologoId, creadoPor);

        if (ordenCalculado <= 0)
            throw new ArgumentOutOfRangeException(nameof(ordenCalculado), "El orden del entre-turno debe ser mayor a cero.");

        return new Turno
        {
            PacienteWalkInNombre = pacienteNombre.Trim(),
            PacienteWalkInTelefono = pacienteTelefono?.Trim() ?? string.Empty,
            OdontologoId = odontologoId,
            Fecha = fecha,
            HoraInicio = null,
            HoraFin = null,
            Tipo = TipoTurno.EntreTurno,
            OrdenEntreTurno = ordenCalculado,
            Estado = EstadoTurno.Pendiente,
            Origen = origen,
            CreadoPor = creadoPor,
            FechaCreacion = DateTime.UtcNow,
            Notas = notas
        };
    }

    public void IniciarAtencion()
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new InvalidOperationException($"No se puede iniciar un turno con estado '{Estado}'.");

        Estado = EstadoTurno.EnCurso;
    }

    public void FinalizarAtencion()
    {
        if (Estado != EstadoTurno.EnCurso)
            throw new InvalidOperationException("Solo se puede finalizar un turno que esté 'EnCurso'.");

        Estado = EstadoTurno.Finalizado;
    }

    public void Cancelar()
    {
        if (Estado == EstadoTurno.Finalizado)
            throw new InvalidOperationException("No se puede cancelar un turno ya finalizado.");

        Estado = EstadoTurno.Cancelado;
    }

    private static void ValidarDatosComunes(string pacienteNombre, int odontologoId, int creadoPor)
    {
        if (string.IsNullOrWhiteSpace(pacienteNombre))
            throw new ArgumentException("El nombre del paciente es obligatorio.", nameof(pacienteNombre));

        if (odontologoId <= 0)
            throw new ArgumentOutOfRangeException(nameof(odontologoId), "Debe asociar un odontólogo válido.");

        if (creadoPor <= 0)
            throw new ArgumentOutOfRangeException(nameof(creadoPor), "El identificador de usuario creador es requerido.");
    }
}
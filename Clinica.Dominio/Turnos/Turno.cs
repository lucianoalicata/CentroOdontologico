using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Turnos;

/// <summary>
/// Turno de atención con un odontólogo.
/// - Programado: ocupa una franja horaria y exige paciente vinculado.
/// - Entre-turno: no ocupa franja; se ordena por OrdenEntreTurno y admite datos provisorios del paciente.
/// No tiene borrado lógico: un turno se anula cancelándolo.
/// </summary>
public class Turno : Entidad
{
    public const int LongitudMaximaMotivo = 500;
    public const int LongitudMaximaNombre = 150;
    public const int LongitudMaximaTelefono = 30;
    public const int LongitudMaximaDni = 20;

    public int OdontologoId { get; private set; }
    public int? PacienteId { get; private set; }
    public TipoTurno Tipo { get; private set; }
    public EstadoTurno Estado { get; private set; }
    public DateOnly Fecha { get; private set; }

    // Solo para turnos programados.
    public TimeOnly? HoraInicio { get; private set; }
    public TimeOnly? HoraFin { get; private set; }

    // Solo para entre-turnos.
    public int? OrdenEntreTurno { get; private set; }

    // Datos provisorios del paciente cuando todavía no tiene ficha.
    public string? PacienteWalkInNombre { get; private set; }
    public string? PacienteWalkInTelefono { get; private set; }
    public string? PacienteWalkInDni { get; private set; }

    public string? Motivo { get; private set; }

    // Trazabilidad.
    public OrigenCreacion Origen { get; private set; }
    public int CreadoPor { get; private set; }
    public string? MotivoCancelacion { get; private set; }
    public int? CanceladoPor { get; private set; }
    public DateTime? FechaCancelacion { get; private set; }

    public bool TienePacienteVinculado => PacienteId.HasValue;
    public bool EstaActivo => Estado is EstadoTurno.Pendiente or EstadoTurno.EnCurso;

    protected Turno() { }

    private Turno(
        int odontologoId,
        DateOnly fecha,
        TipoTurno tipo,
        string? motivo,
        OrigenCreacion origen,
        int creadoPor)
    {
        OdontologoId = Guard.IdValido(odontologoId, "Odontólogo");
        CreadoPor = Guard.IdValido(creadoPor, "Usuario que crea el turno");
        Fecha = fecha;
        Tipo = tipo;
        Origen = origen;
        Motivo = NormalizarOpcional(motivo, LongitudMaximaMotivo, "El motivo");
        Estado = EstadoTurno.Pendiente;
    }

    public static Turno CrearProgramado(
        int odontologoId,
        int pacienteId,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        string? motivo,
        OrigenCreacion origen,
        int creadoPor)
    {
        if (horaFin <= horaInicio)
            throw new DomainException("La hora de fin debe ser posterior a la hora de inicio.");

        return new Turno(odontologoId, fecha, TipoTurno.Programado, motivo, origen, creadoPor)
        {
            PacienteId = Guard.IdValido(pacienteId, "Paciente"),
            HoraInicio = horaInicio,
            HoraFin = horaFin
        };
    }

    /// <summary>
    /// Crea un entre-turno. Si el paciente ya tiene ficha se pasa su Id;
    /// si no, se registran nombre y teléfono (y opcionalmente DNI) como datos provisorios.
    /// </summary>
    public static Turno CrearEntreTurno(
        int odontologoId,
        DateOnly fecha,
        int ordenEntreTurno,
        int? pacienteId,
        string? nombre,
        string? telefono,
        string? dni,
        string? motivo,
        OrigenCreacion origen,
        int creadoPor)
    {
        if (ordenEntreTurno <= 0)
            throw new DomainException("El orden del entre-turno no es válido.");

        var turno = new Turno(odontologoId, fecha, TipoTurno.EntreTurno, motivo, origen, creadoPor)
        {
            OrdenEntreTurno = ordenEntreTurno
        };

        if (pacienteId.HasValue)
        {
            turno.PacienteId = Guard.IdValido(pacienteId.Value, "Paciente");
        }
        else
        {
            turno.PacienteWalkInNombre = NormalizarObligatorio(nombre, LongitudMaximaNombre, "Nombre");
            turno.PacienteWalkInTelefono = NormalizarObligatorio(telefono, LongitudMaximaTelefono, "Teléfono");
            turno.PacienteWalkInDni = NormalizarOpcional(dni, LongitudMaximaDni, "El DNI");
        }

        return turno;
    }

    public void Iniciar()
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new DomainException("Solo se puede iniciar un turno pendiente.");
        if (!TienePacienteVinculado)
            throw new DomainException("Para iniciar la atención el turno debe tener un paciente vinculado.");

        Estado = EstadoTurno.EnCurso;
    }

    public void Finalizar()
    {
        if (Estado != EstadoTurno.EnCurso)
            throw new DomainException("Solo se puede finalizar un turno en curso.");

        Estado = EstadoTurno.Finalizado;
    }

    public void MarcarAusente()
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new DomainException("Solo se puede marcar como ausente un turno pendiente.");

        Estado = EstadoTurno.Ausente;
    }

    public void Cancelar(string motivo, int usuarioId)
    {
        if (Estado != EstadoTurno.Pendiente)
            throw new DomainException("Solo se puede cancelar un turno pendiente.");

        MotivoCancelacion = NormalizarObligatorio(motivo, LongitudMaximaMotivo, "Motivo de cancelación");
        CanceladoPor = Guard.IdValido(usuarioId, "Usuario que cancela");
        FechaCancelacion = DateTime.UtcNow;
        Estado = EstadoTurno.Cancelado;
    }

    /// <summary>
    /// Vincula la ficha de un paciente (existente o recién creada). Los datos provisorios se conservan
    /// como registro de lo informado al pedir el turno.
    /// </summary>
    public void VincularPaciente(int pacienteId)
    {
        if (Estado is EstadoTurno.Cancelado or EstadoTurno.Finalizado)
            throw new DomainException("No se puede vincular un paciente a un turno cancelado o finalizado.");

        PacienteId = Guard.IdValido(pacienteId, "Paciente");
    }

    /// <summary>
    /// Indica si dos turnos programados activos del mismo odontólogo y la misma fecha se superponen.
    /// Los entre-turnos nunca se solapan porque no ocupan franja.
    /// </summary>
    public bool SeSolapaCon(Turno otro)
    {
        if (Tipo != TipoTurno.Programado || otro.Tipo != TipoTurno.Programado) return false;
        if (!EstaActivo || !otro.EstaActivo) return false;
        if (OdontologoId != otro.OdontologoId || Fecha != otro.Fecha) return false;

        return HoraInicio < otro.HoraFin && otro.HoraInicio < HoraFin;
    }

    private static string NormalizarObligatorio(string? valor, int maximo, string campo)
    {
        var limpio = Guard.NoVacio(valor, campo);
        if (limpio.Length > maximo)
            throw new DomainException($"{campo} no puede superar los {maximo} caracteres.");
        return limpio;
    }

    private static string? NormalizarOpcional(string? valor, int maximo, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;

        var limpio = valor.Trim();
        if (limpio.Length > maximo)
            throw new DomainException($"{campo} no puede superar los {maximo} caracteres.");
        return limpio;
    }
}

using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Turnos;

/// <summary>
/// Franja semanal en la que un odontólogo atiende (por ejemplo, lunes de 09:00 a 13:00 con turnos de 30 minutos).
/// Las franjas activas de un mismo odontólogo no pueden solaparse en el mismo día.
/// </summary>
public class HorarioDisponible : EntidadConBorradoLogico
{
    public const int DuracionMinimaTurnoMinutos = 10;
    public const int DuracionMaximaTurnoMinutos = 120;

    public int OdontologoId { get; private set; }
    public DiaSemana DiaSemana { get; private set; }
    public TimeOnly HoraInicio { get; private set; }
    public TimeOnly HoraFin { get; private set; }
    public int DuracionTurnoMinutos { get; private set; }

    protected HorarioDisponible() { }

    public HorarioDisponible(
        int odontologoId,
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        int duracionTurnoMinutos)
    {
        OdontologoId = Guard.IdValido(odontologoId, "Odontólogo");
        AsignarFranja(diaSemana, horaInicio, horaFin, duracionTurnoMinutos);
    }

    /// <summary>Modifica la franja. El odontólogo al que pertenece no cambia.</summary>
    public void Actualizar(
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        int duracionTurnoMinutos)
    {
        AsignarFranja(diaSemana, horaInicio, horaFin, duracionTurnoMinutos);
    }

    /// <summary>
    /// Indica si esta franja se superpone con otra franja activa del mismo odontólogo y el mismo día.
    /// Franjas contiguas (una termina 13:00 y otra empieza 13:00) no se solapan.
    /// </summary>
    public bool SeSolapaCon(HorarioDisponible otro)
    {
        if (EsElMismoQue(otro)) return false;
        if (!Activo || !otro.Activo) return false;
        if (OdontologoId != otro.OdontologoId || DiaSemana != otro.DiaSemana) return false;

        return HoraInicio < otro.HoraFin && otro.HoraInicio < HoraFin;
    }

    /// <summary>
    /// Lanza <see cref="DomainException"/> si esta franja se solapa con alguna de las existentes.
    /// Al editar, la propia franja puede venir en la lista: se ignora.
    /// </summary>
    public void ValidarQueNoSeSolapaCon(IEnumerable<HorarioDisponible> existentes)
    {
        var conflicto = existentes.FirstOrDefault(SeSolapaCon);
        if (conflicto is not null)
            throw new DomainException(
                $"La franja se solapa con otro horario del {NombreDia(conflicto.DiaSemana)} " +
                $"({conflicto.HoraInicio:HH\\:mm} a {conflicto.HoraFin:HH\\:mm}).");
    }

    /// <summary>Día de la semana correspondiente a una fecha, con la numeración del dominio.</summary>
    public static DiaSemana DiaDe(DateOnly fecha) =>
        fecha.DayOfWeek == DayOfWeek.Sunday ? DiaSemana.Domingo : (DiaSemana)(int)fecha.DayOfWeek;

    private void AsignarFranja(
        DiaSemana diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        int duracionTurnoMinutos)
    {
        if (!Enum.IsDefined(diaSemana))
            throw new DomainException("El día de la semana no es válido.");

        if (horaFin <= horaInicio)
            throw new DomainException("La hora de inicio debe ser anterior a la hora de fin.");

        if (duracionTurnoMinutos < DuracionMinimaTurnoMinutos || duracionTurnoMinutos > DuracionMaximaTurnoMinutos)
            throw new DomainException(
                $"La duración del turno debe estar entre {DuracionMinimaTurnoMinutos} y {DuracionMaximaTurnoMinutos} minutos.");

        if ((horaFin - horaInicio).TotalMinutes < duracionTurnoMinutos)
            throw new DomainException("La franja horaria es más corta que la duración de un turno.");

        DiaSemana = diaSemana;
        HoraInicio = horaInicio;
        HoraFin = horaFin;
        DuracionTurnoMinutos = duracionTurnoMinutos;
    }

    private bool EsElMismoQue(HorarioDisponible otro) =>
        ReferenceEquals(this, otro) || (Id != 0 && Id == otro.Id);

    private static string NombreDia(DiaSemana dia) => dia switch
    {
        DiaSemana.Lunes => "lunes",
        DiaSemana.Martes => "martes",
        DiaSemana.Miercoles => "miércoles",
        DiaSemana.Jueves => "jueves",
        DiaSemana.Viernes => "viernes",
        DiaSemana.Sabado => "sábado",
        _ => "domingo"
    };
}

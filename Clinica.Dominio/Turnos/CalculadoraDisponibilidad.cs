namespace Clinica.Dominio.Turnos;

/// <summary>
/// Calcula las franjas libres de un odontólogo en una fecha:
/// las franjas que generan sus horarios de ese día de la semana, menos las ocupadas por turnos programados activos.
/// Los entre-turnos no ocupan franja, por eso no se descuentan.
/// </summary>
public static class CalculadoraDisponibilidad
{
    public static IReadOnlyList<FranjaHoraria> FranjasLibres(
        int odontologoId,
        DateOnly fecha,
        IEnumerable<HorarioDisponible> horarios,
        IEnumerable<Turno> turnos)
    {
        var dia = HorarioDisponible.DiaDe(fecha);

        var ocupadas = turnos
            .Where(t => t.OdontologoId == odontologoId
                        && t.Fecha == fecha
                        && t.Tipo == TipoTurno.Programado
                        && t.EstaActivo)
            .Select(t => new FranjaHoraria(t.HoraInicio!.Value, t.HoraFin!.Value))
            .ToList();

        return horarios
            .Where(h => h.OdontologoId == odontologoId && h.DiaSemana == dia && h.Activo)
            .SelectMany(GenerarFranjas)
            .Where(franja => !ocupadas.Any(ocupada => SeSolapan(franja, ocupada)))
            .Distinct()
            .OrderBy(f => f.Inicio)
            .ToList();
    }

    /// <summary>
    /// Divide un horario en turnos consecutivos de la duración configurada.
    /// Si al final queda un resto menor a un turno, se descarta (ej.: 09:00-10:45 con turnos de 30 → 09:00, 09:30, 10:00).
    /// </summary>
    public static IEnumerable<FranjaHoraria> GenerarFranjas(HorarioDisponible horario)
    {
        var duracion = TimeSpan.FromMinutes(horario.DuracionTurnoMinutos);
        var fin = horario.HoraFin.ToTimeSpan();

        for (var inicio = horario.HoraInicio.ToTimeSpan(); inicio + duracion <= fin; inicio += duracion)
            yield return new FranjaHoraria(TimeOnly.FromTimeSpan(inicio), TimeOnly.FromTimeSpan(inicio + duracion));
    }

    private static bool SeSolapan(FranjaHoraria a, FranjaHoraria b) =>
        a.Inicio < b.Fin && b.Inicio < a.Fin;
}

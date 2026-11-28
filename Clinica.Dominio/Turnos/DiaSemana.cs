namespace Clinica.Dominio.Turnos;

/// <summary>
/// Día de la semana con la numeración que usa la interfaz (1 = Lunes ... 7 = Domingo).
/// Se evita System.DayOfWeek porque numera el domingo como 0.
/// </summary>
public enum DiaSemana
{
    Lunes = 1,
    Martes = 2,
    Miercoles = 3,
    Jueves = 4,
    Viernes = 5,
    Sabado = 6,
    Domingo = 7
}

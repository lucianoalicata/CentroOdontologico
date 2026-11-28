namespace Clinica.Dominio.Turnos;

/// <summary>Intervalo de tiempo dentro de un día, por ejemplo el lugar para un turno de 09:00 a 09:30.</summary>
public readonly record struct FranjaHoraria(TimeOnly Inicio, TimeOnly Fin);

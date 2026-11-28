using Clinica.Aplicacion.Turnos.Dtos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Aplicacion.Turnos.Servicios;

public class DisponibilidadService : IDisponibilidadService
{
    private readonly IHorarioDisponibleRepository _horarios;
    private readonly ITurnoRepository _turnos;

    public DisponibilidadService(IHorarioDisponibleRepository horarios, ITurnoRepository turnos)
    {
        _horarios = horarios;
        _turnos = turnos;
    }

    public async Task<IReadOnlyList<FranjaLibreDto>> ObtenerFranjasLibresAsync(
        int odontologoId, DateOnly fecha, CancellationToken ct = default)
    {
        Guard.IdValido(odontologoId, "Odontólogo");

        var horarios = await _horarios.ListarPorOdontologoAsync(odontologoId, ct);
        if (horarios.Count == 0)
            return Array.Empty<FranjaLibreDto>();

        var turnos = await _turnos.ListarProgramadosActivosAsync(odontologoId, fecha, ct);

        return CalculadoraDisponibilidad
            .FranjasLibres(odontologoId, fecha, horarios, turnos)
            .Select(f => new FranjaLibreDto(TurnoMapeo.AHoraTexto(f.Inicio), TurnoMapeo.AHoraTexto(f.Fin)))
            .ToList();
    }
}

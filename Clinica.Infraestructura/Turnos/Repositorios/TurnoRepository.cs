using Clinica.Dominio.Turnos;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Turnos.Repositorios;

public class TurnoRepository : ITurnoRepository
{
    private readonly ClinicaDbContext _context;

    public TurnoRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Turno turno, CancellationToken ct = default)
    {
        await _context.Turnos.AddAsync(turno, ct);
    }

    public async Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<Turno>> ListarProgramadosActivosAsync(
        int odontologoId, DateOnly fecha, CancellationToken ct = default)
    {
        return await _context.Turnos
            .AsNoTracking()
            .Where(t => t.OdontologoId == odontologoId
                        && t.Fecha == fecha
                        && t.Tipo == TipoTurno.Programado
                        && (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.EnCurso))
            .OrderBy(t => t.HoraInicio)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Turno>> ListarPorOdontologoYFechaAsync(
        int odontologoId, DateOnly fecha, CancellationToken ct = default)
    {
        return await _context.Turnos
            .AsNoTracking()
            .Where(t => t.OdontologoId == odontologoId && t.Fecha == fecha)
            .ToListAsync(ct);
    }

    public async Task<int> ObtenerUltimoOrdenEntreTurnoAsync(
        int odontologoId, DateOnly fecha, CancellationToken ct = default)
    {
        return await _context.Turnos
            .Where(t => t.OdontologoId == odontologoId
                        && t.Fecha == fecha
                        && t.OrdenEntreTurno != null)
            .MaxAsync(t => (int?)t.OrdenEntreTurno, ct) ?? 0;
    }
}

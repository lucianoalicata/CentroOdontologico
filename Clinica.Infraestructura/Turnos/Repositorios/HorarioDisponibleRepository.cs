using Clinica.Dominio.Turnos;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Turnos.Repositorios;

public class HorarioDisponibleRepository : IHorarioDisponibleRepository
{
    private readonly ClinicaDbContext _context;

    public HorarioDisponibleRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(HorarioDisponible horario, CancellationToken ct = default)
    {
        await _context.HorariosDisponibles.AddAsync(horario, ct);
    }

    public async Task<HorarioDisponible?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.HorariosDisponibles.FirstOrDefaultAsync(h => h.Id == id, ct);
    }

    public async Task<IReadOnlyList<HorarioDisponible>> ListarPorOdontologoAsync(
        int odontologoId, CancellationToken ct = default)
    {
        var horarios = await _context.HorariosDisponibles
            .AsNoTracking()
            .Where(h => h.OdontologoId == odontologoId)
            .ToListAsync(ct);

        // El día se guarda como texto: se ordena en memoria para respetar Lunes → Domingo
        return horarios
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .ToList();
    }
}

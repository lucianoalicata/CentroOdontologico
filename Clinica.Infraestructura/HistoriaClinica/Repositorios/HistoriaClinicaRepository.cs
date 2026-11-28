using Clinica.Dominio.HistoriaClinica;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.HistoriaClinica.Repositorios;

public class HistoriaClinicaRepository : IHistoriaClinicaRepository
{
    private readonly ClinicaDbContext _context;

    public HistoriaClinicaRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(HistoriaClinicaPaciente historia, CancellationToken ct = default)
    {
        await _context.Set<HistoriaClinicaPaciente>().AddAsync(historia, ct);
    }

    public async Task<HistoriaClinicaPaciente?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Set<HistoriaClinicaPaciente>()
            .FirstOrDefaultAsync(h => h.Id == id, ct);
    }

    public async Task<HistoriaClinicaPaciente?> ObtenerPorPacienteIdAsync(int pacienteId, CancellationToken ct = default)
    {
        return await _context.Set<HistoriaClinicaPaciente>()
            .FirstOrDefaultAsync(h => h.PacienteId == pacienteId, ct);
    }

    public async Task<HistoriaClinicaPaciente?> ObtenerPorPacienteIdConEntradasAsync(int pacienteId, CancellationToken ct = default)
    {
        return await _context.Set<HistoriaClinicaPaciente>()
            .Include(h => h.Entradas)
                .ThenInclude(e => e.Prestaciones)
            .FirstOrDefaultAsync(h => h.PacienteId == pacienteId, ct);
    }

    public async Task<EntradaHistoriaClinica?> ObtenerEntradaPorIdAsync(int entradaId, CancellationToken ct = default)
    {
        return await _context.Set<EntradaHistoriaClinica>()
            .AsNoTracking()
            .Include(e => e.Prestaciones)
            .FirstOrDefaultAsync(e => e.Id == entradaId, ct);
    }

    public async Task<IReadOnlyList<EntradaHistoriaClinica>> ListarEntradasAsync(int historiaClinicaId, CancellationToken ct = default)
    {
        return await _context.Set<EntradaHistoriaClinica>()
            .AsNoTracking()
            .Include(e => e.Prestaciones)
            .Where(e => e.HistoriaClinicaId == historiaClinicaId)
            .OrderBy(e => e.FechaCreacion)
            .ThenBy(e => e.Id)
            .ToListAsync(ct);
    }
}

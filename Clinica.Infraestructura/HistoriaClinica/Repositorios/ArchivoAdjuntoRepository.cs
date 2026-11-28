using Clinica.Dominio.HistoriaClinica;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.HistoriaClinica.Repositorios;

public class ArchivoAdjuntoRepository : IArchivoAdjuntoRepository
{
    private readonly ClinicaDbContext _context;

    public ArchivoAdjuntoRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(ArchivoAdjunto adjunto, CancellationToken ct = default)
    {
        await _context.Set<ArchivoAdjunto>().AddAsync(adjunto, ct);
    }

    public async Task<ArchivoAdjunto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Set<ArchivoAdjunto>()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<ArchivoAdjunto>> ListarPorEntradaAsync(int entradaId, CancellationToken ct = default)
    {
        return await _context.Set<ArchivoAdjunto>()
            .AsNoTracking()
            .Where(a => a.EntradaHistoriaClinicaId == entradaId)
            .OrderBy(a => a.FechaCreacion)
            .ThenBy(a => a.Id)
            .ToListAsync(ct);
    }
}

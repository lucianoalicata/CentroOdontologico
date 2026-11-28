using Clinica.Dominio.HistoriaClinica;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.HistoriaClinica.Repositorios;

public class PrestacionRepository : IPrestacionRepository
{
    private readonly ClinicaDbContext _context;

    public PrestacionRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Prestacion prestacion, CancellationToken ct = default)
    {
        await _context.Prestaciones.AddAsync(prestacion, ct);
    }

    public async Task<Prestacion?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Prestaciones.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Prestacion?> ObtenerPorIdIncluyendoInactivasAsync(int id, CancellationToken ct = default)
    {
        return await _context.Prestaciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Prestacion?> ObtenerPorCodigoIncluyendoInactivasAsync(string codigo, CancellationToken ct = default)
    {
        var limpio = codigo.Trim();

        return await _context.Prestaciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Codigo == limpio, ct);
    }

    public async Task<IReadOnlyList<Prestacion>> ListarAsync(
        string? filtro,
        CategoriaPrestacion? categoria,
        bool incluirInactivas,
        CancellationToken ct = default)
    {
        var query = _context.Prestaciones.AsNoTracking().AsQueryable();

        if (incluirInactivas)
            query = query.IgnoreQueryFilters();

        if (categoria.HasValue)
            query = query.Where(p => p.Categoria == categoria.Value);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termino = filtro.Trim().ToLower();
            query = query.Where(p =>
                p.Codigo.ToLower().Contains(termino) ||
                p.Titulo.ToLower().Contains(termino));
        }

        return await query.OrderBy(p => p.Codigo).ToListAsync(ct);
    }
}

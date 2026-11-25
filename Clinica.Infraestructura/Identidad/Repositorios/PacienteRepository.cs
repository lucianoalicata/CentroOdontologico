using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Identidad.Repositorios;

public class PacienteRepository : IPacienteRepository
{
    private readonly ClinicaDbContext _context;

    public PacienteRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(PacientePerfil paciente, CancellationToken ct = default)
    {
        await _context.Pacientes.AddAsync(paciente, ct);
    }

    public async Task<PacientePerfil?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PacientePerfil?> ObtenerPorDniIncluyendoInactivosAsync(string dni, CancellationToken ct = default)
    {
        var dniLimpio = dni.Trim().Replace(".", "").Replace(" ", "");

        return await _context.Pacientes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Dni == dniLimpio, ct);
    }

    public async Task<PacientePerfil?> ObtenerPorIdIncluyendoInactivosAsync(int id, CancellationToken ct = default)
    {
        return await _context.Pacientes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<ResultadoPaginado<PacientePerfil>> BuscarPaginadoAsync(
        string? filtro,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        pageIndex = pageIndex < 1 ? 1 : pageIndex;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var query = _context.Pacientes
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termino = filtro.Trim().ToLower();
            var terminoDni = termino.Replace(".", "");

            query = query.Where(p =>
                p.Nombre.ToLower().Contains(termino) ||
                p.Apellido.ToLower().Contains(termino) ||
                p.Dni.Contains(terminoDni));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ResultadoPaginado<PacientePerfil>(items, totalItems, pageIndex, pageSize);
    }
}
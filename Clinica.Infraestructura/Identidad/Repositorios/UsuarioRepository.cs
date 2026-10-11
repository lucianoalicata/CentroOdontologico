using Clinica.Dominio.Identidad;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Identidad.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ClinicaDbContext _db;

    public UsuarioRepository(ClinicaDbContext db) => _db = db;

    // Include(Roles): sin esto EF no carga los roles y el usuario parecería no tener ninguno.
    // El filtro global de borrado lógico ya excluye a los usuarios dados de baja.
    public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default) =>
        _db.Set<Usuario>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Set<Usuario>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}
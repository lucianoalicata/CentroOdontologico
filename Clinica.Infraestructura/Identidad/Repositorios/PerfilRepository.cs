using Clinica.Dominio.Identidad;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Identidad.Repositorios;

public class PerfilRepository : IPerfilRepository
{
    private readonly ClinicaDbContext _db;

    public PerfilRepository(ClinicaDbContext db) => _db = db;

    public Task<PacientePerfil?> ObtenerPacientePorUsuarioIdAsync(int usuarioId, CancellationToken ct = default) =>
        _db.Set<PacientePerfil>().FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, ct);

    public Task<OdontologoPerfil?> ObtenerOdontologoPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default) =>
        _db.Set<OdontologoPerfil>().FirstOrDefaultAsync(o => o.UsuarioId == usuarioId, ct);

    public Task<SecretariaPerfil?> ObtenerSecretariaPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default) =>
        _db.Set<SecretariaPerfil>().FirstOrDefaultAsync(s => s.UsuarioId == usuarioId, ct);
}
using Clinica.Aplicacion.Turnos.Puertos;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Turnos.Consultas;

public class ConsultaPerfiles : IConsultaPerfiles
{
    private readonly ClinicaDbContext _db;

    public ConsultaPerfiles(ClinicaDbContext db)
    {
        _db = db;
    }

    // El filtro global de borrado lógico deja afuera a los perfiles dados de baja
    public Task<bool> ExisteOdontologoActivoAsync(int odontologoId, CancellationToken ct = default) =>
        _db.Odontologos.AnyAsync(o => o.Id == odontologoId, ct);

    public Task<bool> ExistePacienteActivoAsync(int pacienteId, CancellationToken ct = default) =>
        _db.Pacientes.AnyAsync(p => p.Id == pacienteId, ct);
}

using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Puertos;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Identidad.Repositorios;

public class OdontologoConsulta : IOdontologoConsulta
{
    private readonly ClinicaDbContext _db;

    public OdontologoConsulta(ClinicaDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OdontologoActivoDto>> ListarActivosAsync(CancellationToken ct = default)
    {
        return await (
            from odontologo in _db.Odontologos.AsNoTracking()
            join usuario in _db.Usuarios.AsNoTracking()
                on odontologo.UsuarioId equals usuario.Id
            orderby usuario.Apellido, usuario.Nombre
            select new OdontologoActivoDto(
                odontologo.Id,
                usuario.Nombre,
                usuario.Apellido,
                odontologo.Matricula,
                odontologo.Especialidad)
        ).ToListAsync(ct);
    }
}
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Comun.Puertos;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Turnos.Dtos;
using Clinica.Aplicacion.Turnos.Puertos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Turnos;

namespace Clinica.Aplicacion.Turnos.Servicios;

/// <summary>
/// Registra un entre-turno: no ocupa franja y se atiende por orden de llegada
/// (máximo orden del día y del odontólogo más uno).
/// - Personal (secretaría o administrador): en la agenda de cualquier odontólogo.
/// - Odontólogo: solo en su propia agenda.
/// - Paciente: no puede crear entre-turnos.
/// Si el paciente no tiene ficha se guardan sus datos provisorios; si el DNI informado ya pertenece
/// a una ficha, se devuelve como sugerencia para vincularla (no se vincula sola).
/// </summary>
public class CrearEntreTurnoUseCase
{
    private readonly ITurnoRepository _turnos;
    private readonly IConsultaPerfiles _perfiles;
    private readonly IUsuarioActual _usuario;
    private readonly IReloj _reloj;
    private readonly IUnitOfWork _unitOfWork;

    public CrearEntreTurnoUseCase(
        ITurnoRepository turnos,
        IConsultaPerfiles perfiles,
        IUsuarioActual usuario,
        IReloj reloj,
        IUnitOfWork unitOfWork)
    {
        _turnos = turnos;
        _perfiles = perfiles;
        _usuario = usuario;
        _reloj = reloj;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntreTurnoCreadoDto> EjecutarAsync(CrearEntreTurnoDto dto, CancellationToken ct = default)
    {
        Guard.IdValido(dto.OdontologoId, "Odontólogo");
        var origen = ResolverOrigenYPermisos(dto);

        var hoy = DateOnly.FromDateTime(_reloj.AhoraEnClinica);
        if (dto.Fecha < hoy)
            throw new DomainException("No se puede registrar un entre-turno en una fecha que ya pasó.");

        if (!await _perfiles.ExisteOdontologoActivoAsync(dto.OdontologoId, ct))
            throw new NoEncontradoException("el odontólogo");

        PacienteSugeridoDto? sugerido = null;
        var dni = NormalizarDni(dto.Dni);

        if (dto.PacienteId.HasValue)
        {
            if (!await _perfiles.ExistePacienteActivoAsync(dto.PacienteId.Value, ct))
                throw new NoEncontradoException("el paciente");
        }
        else if (dni is not null)
        {
            sugerido = await _perfiles.BuscarPacienteActivoPorDniAsync(dni, ct);
        }

        var orden = await _turnos.ObtenerUltimoOrdenEntreTurnoAsync(dto.OdontologoId, dto.Fecha, ct) + 1;

        var turno = Turno.CrearEntreTurno(
            dto.OdontologoId,
            dto.Fecha,
            orden,
            dto.PacienteId,
            dto.Nombre,
            dto.Telefono,
            dni,
            dto.Motivo,
            origen,
            _usuario.UsuarioId);

        await _turnos.AgregarAsync(turno, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);

        return new EntreTurnoCreadoDto(TurnoMapeo.ADto(turno), sugerido);
    }

    private OrigenCreacion ResolverOrigenYPermisos(CrearEntreTurnoDto dto)
    {
        if (PermisosTurno.EsPersonal(_usuario))
            return PermisosTurno.OrigenDelPersonal(dto.Origen);

        if (PermisosTurno.EsOdontologo(_usuario))
        {
            if (_usuario.OdontologoId != dto.OdontologoId)
                throw new AccesoDenegadoException("Solo podés registrar entre-turnos en tu propia agenda.");
            return OrigenCreacion.Odontologo;
        }

        throw new AccesoDenegadoException("Los entre-turnos los registra la secretaría o el odontólogo.");
    }

    // Mismo criterio que la ficha del paciente: sin puntos ni espacios
    private static string? NormalizarDni(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return null;
        return dni.Trim().Replace(".", "").Replace(" ", "");
    }
}

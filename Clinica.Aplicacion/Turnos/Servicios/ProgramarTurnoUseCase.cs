using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Comun.Puertos;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Turnos.Dtos;
using Clinica.Aplicacion.Turnos.Puertos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;
using Clinica.Dominio.Turnos;

namespace Clinica.Aplicacion.Turnos.Servicios;

/// <summary>
/// Programa un turno en una franja libre del odontólogo.
/// - Personal (secretaría o administrador): para cualquier paciente; elige si el pedido fue telefónico o presencial.
/// - Odontólogo: solo en su propia agenda.
/// - Paciente: solo para sí mismo (origen Web).
/// </summary>
public class ProgramarTurnoUseCase
{
    private readonly ITurnoRepository _turnos;
    private readonly IHorarioDisponibleRepository _horarios;
    private readonly IConsultaPerfiles _perfiles;
    private readonly IUsuarioActual _usuario;
    private readonly IReloj _reloj;
    private readonly IUnitOfWork _unitOfWork;

    public ProgramarTurnoUseCase(
        ITurnoRepository turnos,
        IHorarioDisponibleRepository horarios,
        IConsultaPerfiles perfiles,
        IUsuarioActual usuario,
        IReloj reloj,
        IUnitOfWork unitOfWork)
    {
        _turnos = turnos;
        _horarios = horarios;
        _perfiles = perfiles;
        _usuario = usuario;
        _reloj = reloj;
        _unitOfWork = unitOfWork;
    }

    public async Task<TurnoDto> EjecutarAsync(ProgramarTurnoDto dto, CancellationToken ct = default)
    {
        Guard.IdValido(dto.OdontologoId, "Odontólogo");
        Guard.IdValido(dto.PacienteId, "Paciente");
        var horaInicio = TurnoMapeo.LeerHora(dto.HoraInicio, "La hora de inicio");

        var origen = ResolverOrigenYPermisos(dto);

        var ahora = _reloj.AhoraEnClinica;
        if (dto.Fecha.ToDateTime(horaInicio) <= ahora)
            throw new DomainException("No se puede programar un turno en una fecha u hora que ya pasó.");

        if (!await _perfiles.ExisteOdontologoActivoAsync(dto.OdontologoId, ct))
            throw new NoEncontradoException("el odontólogo");

        if (!await _perfiles.ExistePacienteActivoAsync(dto.PacienteId, ct))
            throw new NoEncontradoException("el paciente");

        var franja = await BuscarFranjaLibreAsync(dto.OdontologoId, dto.Fecha, horaInicio, ct)
            ?? throw new DomainException("El horario elegido no está disponible. Elegí otra franja libre.");

        var turno = Turno.CrearProgramado(
            dto.OdontologoId,
            dto.PacienteId,
            dto.Fecha,
            franja.Inicio,
            franja.Fin,
            dto.Motivo,
            origen,
            _usuario.UsuarioId);

        await _turnos.AgregarAsync(turno, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);

        return TurnoMapeo.ADto(turno);
    }

    private OrigenCreacion ResolverOrigenYPermisos(ProgramarTurnoDto dto)
    {
        if (_usuario.TieneRol(nameof(RolUsuario.Secretaria)) || _usuario.TieneRol(nameof(RolUsuario.Administrador)))
        {
            var origen = dto.Origen ?? OrigenCreacion.PresencialSecretaria;
            if (origen is not (OrigenCreacion.TelefonoSecretaria or OrigenCreacion.PresencialSecretaria))
                throw new DomainException("El origen del turno debe ser telefónico o presencial.");
            return origen;
        }

        if (_usuario.TieneRol(nameof(RolUsuario.Odontologo)))
        {
            if (_usuario.OdontologoId != dto.OdontologoId)
                throw new AccesoDenegadoException("Solo podés programar turnos en tu propia agenda.");
            return OrigenCreacion.Odontologo;
        }

        if (_usuario.TieneRol(nameof(RolUsuario.Paciente)))
        {
            if (_usuario.PacienteId != dto.PacienteId)
                throw new AccesoDenegadoException("Solo podés programar turnos para vos.");
            return OrigenCreacion.Web;
        }

        throw new AccesoDenegadoException();
    }

    private async Task<FranjaHoraria?> BuscarFranjaLibreAsync(
        int odontologoId, DateOnly fecha, TimeOnly horaInicio, CancellationToken ct)
    {
        var horarios = await _horarios.ListarPorOdontologoAsync(odontologoId, ct);
        var ocupados = await _turnos.ListarProgramadosActivosAsync(odontologoId, fecha, ct);

        return CalculadoraDisponibilidad
            .FranjasLibres(odontologoId, fecha, horarios, ocupados)
            .Where(f => f.Inicio == horaInicio)
            .Cast<FranjaHoraria?>()
            .FirstOrDefault();
    }
}

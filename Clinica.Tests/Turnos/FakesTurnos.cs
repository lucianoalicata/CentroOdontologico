using Clinica.Aplicacion.Comun.Puertos;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Turnos.Puertos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;
using Clinica.Dominio.Turnos;

namespace Clinica.Tests.Turnos;

// Implementaciones falsas compartidas por las pruebas del módulo de Turnos (sin base de datos).

internal static class AsignadorId
{
    public static T ConId<T>(T entidad, int id) where T : Entidad
    {
        typeof(Entidad).GetProperty(nameof(Entidad.Id))!.SetValue(entidad, id);
        return entidad;
    }
}

internal class FakeTurnos : ITurnoRepository
{
    private int _siguienteId = 1;

    public List<Turno> Lista { get; } = new();
    public bool FueConsultado { get; private set; }

    /// <summary>Agrega un turno ya existente (como si estuviera guardado), asignándole Id.</summary>
    public Turno Sembrar(Turno turno)
    {
        Lista.Add(AsignadorId.ConId(turno, _siguienteId++));
        return turno;
    }

    public Task AgregarAsync(Turno turno, CancellationToken ct = default)
    {
        Sembrar(turno);
        return Task.CompletedTask;
    }

    public Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<Turno>> ListarProgramadosActivosAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default)
    {
        FueConsultado = true;
        return Task.FromResult<IReadOnlyList<Turno>>(
            Lista.Where(t => t.OdontologoId == odontologoId && t.Fecha == fecha
                             && t.Tipo == TipoTurno.Programado && t.EstaActivo).ToList());
    }

    public Task<IReadOnlyList<Turno>> ListarPorOdontologoYFechaAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Turno>>(
            Lista.Where(t => t.OdontologoId == odontologoId && t.Fecha == fecha).ToList());

    public Task<int> ObtenerUltimoOrdenEntreTurnoAsync(int odontologoId, DateOnly fecha, CancellationToken ct = default) =>
        Task.FromResult(Lista
            .Where(t => t.OdontologoId == odontologoId && t.Fecha == fecha && t.OrdenEntreTurno.HasValue)
            .Select(t => t.OrdenEntreTurno!.Value)
            .DefaultIfEmpty(0)
            .Max());
}

internal class FakeHorarios : IHorarioDisponibleRepository
{
    public List<HorarioDisponible> Lista { get; } = new();

    public Task AgregarAsync(HorarioDisponible horario, CancellationToken ct = default)
    {
        Lista.Add(horario);
        return Task.CompletedTask;
    }

    public Task<HorarioDisponible?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(h => h.Id == id && h.Activo));

    public Task<IReadOnlyList<HorarioDisponible>> ListarPorOdontologoAsync(int odontologoId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HorarioDisponible>>(
            Lista.Where(h => h.OdontologoId == odontologoId && h.Activo).ToList());
}

internal class FakeConsultaPerfiles : IConsultaPerfiles
{
    public HashSet<int> OdontologosActivos { get; } = new();
    public HashSet<int> PacientesActivos { get; } = new();

    public Task<bool> ExisteOdontologoActivoAsync(int odontologoId, CancellationToken ct = default) =>
        Task.FromResult(OdontologosActivos.Contains(odontologoId));

    public Task<bool> ExistePacienteActivoAsync(int pacienteId, CancellationToken ct = default) =>
        Task.FromResult(PacientesActivos.Contains(pacienteId));
}

internal class FakeUsuarioActual : IUsuarioActual
{
    private readonly List<string> _roles = new();

    public bool EstaAutenticado => true;
    public int UsuarioId { get; private set; }
    public IReadOnlyList<string> Roles => _roles;
    public int? PacienteId { get; private set; }
    public int? OdontologoId { get; private set; }
    public int? SecretariaId { get; private set; }

    public bool TieneRol(string rol) => _roles.Contains(rol);

    public static FakeUsuarioActual Secretaria(int usuarioId = 50) =>
        new() { UsuarioId = usuarioId, SecretariaId = 1, _roles = { nameof(RolUsuario.Secretaria) } };

    public static FakeUsuarioActual Administrador(int usuarioId = 60) =>
        new() { UsuarioId = usuarioId, _roles = { nameof(RolUsuario.Administrador) } };

    public static FakeUsuarioActual Odontologo(int odontologoId, int usuarioId = 70) =>
        new() { UsuarioId = usuarioId, OdontologoId = odontologoId, _roles = { nameof(RolUsuario.Odontologo) } };

    public static FakeUsuarioActual Paciente(int pacienteId, int usuarioId = 80) =>
        new() { UsuarioId = usuarioId, PacienteId = pacienteId, _roles = { nameof(RolUsuario.Paciente) } };

    public static FakeUsuarioActual SinRoles(int usuarioId = 90) => new() { UsuarioId = usuarioId };
}

internal class FakeReloj : IReloj
{
    public FakeReloj(DateTime ahora) => AhoraEnClinica = ahora;
    public DateTime AhoraEnClinica { get; set; }
}

internal class FakeUnitOfWork : IUnitOfWork
{
    public int Guardados { get; private set; }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        Guardados++;
        return Task.FromResult(1);
    }
}

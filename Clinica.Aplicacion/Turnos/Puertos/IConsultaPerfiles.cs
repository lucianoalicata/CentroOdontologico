namespace Clinica.Aplicacion.Turnos.Puertos;

/// <summary>
/// Lectura de datos de Identidad que necesita el módulo de Turnos.
/// Turnos referencia a pacientes y odontólogos solo por Id.
/// </summary>
public interface IConsultaPerfiles
{
    Task<bool> ExisteOdontologoActivoAsync(int odontologoId, CancellationToken ct = default);
    Task<bool> ExistePacienteActivoAsync(int pacienteId, CancellationToken ct = default);
}

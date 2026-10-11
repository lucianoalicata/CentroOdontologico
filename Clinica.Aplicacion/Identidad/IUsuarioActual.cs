namespace Clinica.Aplicacion.Identidad;

// "Quién está haciendo este request". Los casos de uso de todos los módulos la reciben
// por inyección para aplicar reglas como "un paciente solo ve sus propios turnos".
public interface IUsuarioActual
{
    bool EstaAutenticado { get; }

    // Lanza CredencialesInvalidasException (401) si no hay sesión
    int UsuarioId { get; }

    IReadOnlyList<string> Roles { get; }
    int? PacienteId { get; }
    int? OdontologoId { get; }
    int? SecretariaId { get; }

    bool TieneRol(string rol);
}
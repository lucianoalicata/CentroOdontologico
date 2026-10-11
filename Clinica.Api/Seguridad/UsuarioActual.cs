using System.Security.Claims;
using Clinica.Aplicacion.Identidad;
using Clinica.Aplicacion.Identidad.Excepciones;

namespace Clinica.Api.Seguridad;

// Implementación de IUsuarioActual: lee los claims del token del request actual.
public class UsuarioActual : IUsuarioActual
{
    private readonly ClaimsPrincipal? _usuario;

    public UsuarioActual(IHttpContextAccessor accessor) => _usuario = accessor.HttpContext?.User;

    public bool EstaAutenticado => _usuario?.Identity?.IsAuthenticated == true;

    public int UsuarioId => LeerEntero(ClaimNombres.UsuarioId)
        ?? throw new CredencialesInvalidasException("No hay una sesión iniciada.");

    public IReadOnlyList<string> Roles =>
        _usuario?.FindAll(ClaimNombres.Rol).Select(c => c.Value).ToList() ?? new List<string>();

    public int? PacienteId => LeerEntero(ClaimNombres.PacienteId);
    public int? OdontologoId => LeerEntero(ClaimNombres.OdontologoId);
    public int? SecretariaId => LeerEntero(ClaimNombres.SecretariaId);

    public bool TieneRol(string rol) => Roles.Contains(rol);

    private int? LeerEntero(string claim)
    {
        var valor = _usuario?.FindFirst(claim)?.Value;
        return int.TryParse(valor, out var numero) ? numero : null;
    }
}
using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Identidad;

public class Usuario : EntidadConBorradoLogico
{
    private readonly List<UsuarioRol> _roles = new();

    public string Nombre { get; private set; } = null!;
    public string Apellido { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public IReadOnlyCollection<UsuarioRol> Roles => _roles;

    protected Usuario() { }

    public Usuario(string nombre, string apellido, string email, string passwordHash, RolUsuario rolInicial)
    {
        Nombre = Guard.NoVacio(nombre, "Nombre");
        Apellido = Guard.NoVacio(apellido, "Apellido");
        Email = NormalizarEmail(email);
        PasswordHash = Guard.NoVacio(passwordHash, "Contraseña");
        AsignarRol(rolInicial);
    }

    public bool TieneRol(RolUsuario rol) => _roles.Any(r => r.Rol == rol);

    public void AsignarRol(RolUsuario rol)
    {
        if (!Enum.IsDefined(rol))
            throw new DomainException("El rol no es válido.");
        if (TieneRol(rol)) return;
        _roles.Add(new UsuarioRol(rol));
    }

    public void QuitarRol(RolUsuario rol)
    {
        var existente = _roles.FirstOrDefault(r => r.Rol == rol);
        if (existente is null) return;
        if (_roles.Count == 1)
            throw new DomainException("El usuario debe conservar al menos un rol.");
        _roles.Remove(existente);
    }

    public void ActualizarDatosBasicos(string nombre, string apellido)
    {
        Nombre = Guard.NoVacio(nombre, "Nombre");
        Apellido = Guard.NoVacio(apellido, "Apellido");
    }

    public void CambiarEmail(string nuevoEmail) => Email = NormalizarEmail(nuevoEmail);

    public void CambiarPassword(string nuevoHash) =>
        PasswordHash = Guard.NoVacio(nuevoHash, "Contraseña");

    private static string NormalizarEmail(string email)
    {
        var limpio = Guard.NoVacio(email, "Email").ToLowerInvariant();
        var partes = limpio.Split('@');
        if (partes.Length != 2 || partes[0].Length == 0 || !partes[1].Contains('.'))
            throw new DomainException("El email no tiene un formato válido.");
        return limpio;
    }
}
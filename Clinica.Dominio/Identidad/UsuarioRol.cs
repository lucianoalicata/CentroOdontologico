namespace Clinica.Dominio.Identidad;

public class UsuarioRol
{
    public int UsuarioId { get; private set; }
    public RolUsuario Rol { get; private set; }

    protected UsuarioRol() { }

    internal UsuarioRol(RolUsuario rol, int usuarioId = 0)
    {
        Rol = rol;
        UsuarioId = usuarioId;
    }
}
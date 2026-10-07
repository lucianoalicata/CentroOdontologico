using Clinica.Dominio.Pacientes;

namespace Clinica.Dominio;

public class Usuario
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Rol Rol { get; private set; }
    public bool Activo { get; private set; } = true;

    // Constructor sin parámetros para Entity Framework Core
    protected Usuario() { }

    public Usuario(string nombre, string apellido, string email, Rol rol)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es requerido.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido es requerido.", nameof(apellido));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es requerido.", nameof(email));

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Email = email.Trim().ToLowerInvariant();
        Rol = rol;
        Activo = true;
    }
}
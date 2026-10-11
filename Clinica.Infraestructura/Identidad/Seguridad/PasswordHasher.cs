using Clinica.Aplicacion.Identidad.Puertos;

namespace Clinica.Infraestructura.Identidad.Seguridad;

// Implementación real del puerto IPasswordHasher usando BCrypt.
// BCrypt agrega una "sal" aleatoria a cada hash, por eso dos hashes de la misma
// contraseña son distintos, y es lento a propósito para frenar ataques de fuerza bruta.
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hashear(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verificar(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or ArgumentException)
        {
            return false; // el hash guardado tiene un formato inválido
        }
    }
}
namespace Clinica.Aplicacion.Identidad.Excepciones;

// Se traduce a HTTP 401 en la API (no es una DomainException, que da 400).
public class CredencialesInvalidasException : Exception
{
    public CredencialesInvalidasException(string? mensaje = null)
        : base(mensaje ?? "Email o contraseña incorrectos.") { }
}
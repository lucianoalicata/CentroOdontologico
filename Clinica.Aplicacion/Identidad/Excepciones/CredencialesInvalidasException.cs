namespace Clinica.Aplicacion.Identidad.Excepciones;

public class CredencialesInvalidasException : Exception
{
    public CredencialesInvalidasException(string? mensaje = null)
        : base(mensaje ?? "Email o contraseña incorrectos.") { }
}
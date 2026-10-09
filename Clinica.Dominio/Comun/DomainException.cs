namespace Clinica.Dominio.Comun;

public class DomainException : Exception
{
    public DomainException(string mensaje) : base(mensaje) { }
}
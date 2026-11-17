namespace Clinica.Aplicacion.Comun.Excepciones
{
    public class AccesoDenegadoException : Exception
    {
        public AccesoDenegadoException(string? mensaje = null)
            : base(mensaje ?? "No tiene permisos para realizar esta acción.") { }
    }
}

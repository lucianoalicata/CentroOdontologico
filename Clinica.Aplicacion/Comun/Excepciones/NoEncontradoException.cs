namespace Clinica.Aplicacion.Comun.Excepciones
{
    public class NoEncontradoException : Exception
    {
        public NoEncontradoException(string recurso)
            : base("No se encontró ${recurso}.") { }
    }
}

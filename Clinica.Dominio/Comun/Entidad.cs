namespace Clinica.Dominio.Comun;

public abstract class Entidad
{
    public int Id { get; protected set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaModificacion { get; private set; }
}
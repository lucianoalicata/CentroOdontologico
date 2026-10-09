namespace Clinica.Dominio.Comun;

public abstract class EntidadConBorradoLogico : Entidad, IBorradoLogico
{
    public bool Activo { get; private set; } = true;
    public DateTime? FechaBaja { get; private set; }

    public void Desactivar()
    {
        if (!Activo) return; 
        Activo = false;
        FechaBaja = DateTime.UtcNow;
    }

    public void Reactivar()
    {
        Activo = true;
        FechaBaja = null;
    }
}
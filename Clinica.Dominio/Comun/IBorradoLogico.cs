namespace Clinica.Dominio.Comun;

public interface IBorradoLogico
{
    bool Activo { get; }
    DateTime? FechaBaja { get; }
    void Desactivar();
    void Reactivar();
}
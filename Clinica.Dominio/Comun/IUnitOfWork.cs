namespace Clinica.Dominio.Comun;

public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
namespace Clinica.Dominio.Identidad;

// Contrato de persistencia definido por el dominio; lo implementa Infraestructura.
public interface IUsuarioRepository
{
    // Devuelve el usuario con sus roles cargados, o null si no existe o está dado de baja.
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
}
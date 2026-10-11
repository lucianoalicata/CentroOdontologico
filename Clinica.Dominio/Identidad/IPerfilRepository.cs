namespace Clinica.Dominio.Identidad;

public interface IPerfilRepository
{
    Task<PacientePerfil?> ObtenerPacientePorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
    Task<OdontologoPerfil?> ObtenerOdontologoPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
    Task<SecretariaPerfil?> ObtenerSecretariaPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default);
}
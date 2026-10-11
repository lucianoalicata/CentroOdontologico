using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Identidad;

public class SecretariaPerfil : EntidadConBorradoLogico
{
    public int UsuarioId { get; private set; }
    public string Legajo { get; private set; } = null!;

    protected SecretariaPerfil() { }

    public SecretariaPerfil(int usuarioId, string legajo)
    {
        UsuarioId = Guard.IdValido(usuarioId, nameof(usuarioId));
        Legajo = Guard.NoVacio(legajo, nameof(legajo));
    }
}
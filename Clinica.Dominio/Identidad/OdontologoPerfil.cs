using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Identidad;

public class OdontologoPerfil : EntidadConBorradoLogico
{
    public int UsuarioId { get; private set; }
    public string Matricula { get; private set; } = null!;
    public string Especialidad { get; private set; } = null!;

    protected OdontologoPerfil() { }

    public OdontologoPerfil(int usuarioId, string matricula, string especialidad)
    {
        UsuarioId = Guard.IdValido(usuarioId, nameof(usuarioId));
        Matricula = Guard.NoVacio(matricula, nameof(matricula));
        Especialidad = Guard.NoVacio(especialidad, nameof(especialidad));
    }
}
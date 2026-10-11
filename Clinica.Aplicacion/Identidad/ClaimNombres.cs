namespace Clinica.Aplicacion.Identidad;

// Nombres de los claims del token. Un único lugar para quien los genera (JwtProvider)
// y para quien los lee (la API).
public static class ClaimNombres
{
    public const string UsuarioId = "sub";   // claim estándar JWT: id del usuario
    public const string Rol = "role";
    public const string Nombre = "nombre";
    public const string Apellido = "apellido";
    public const string PacienteId = "pacienteId";
    public const string OdontologoId = "odontologoId";
    public const string SecretariaId = "secretariaId";
}
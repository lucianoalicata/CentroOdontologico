namespace Clinica.Infraestructura.Identidad.Seguridad;

// Se llena desde la sección "Jwt" de la configuración.
// SecretKey NO va en appsettings.json: usar user-secrets o variables de entorno.
public class JwtSettings
{
    public const string Seccion = "Jwt";

    public string SecretKey { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpirationMinutes { get; set; } = 120;
}
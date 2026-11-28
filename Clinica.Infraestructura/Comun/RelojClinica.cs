using Clinica.Aplicacion.Comun.Puertos;

namespace Clinica.Infraestructura.Comun;

/// <summary>Reloj real con la zona horaria de la clínica (Argentina, UTC-3).</summary>
public class RelojClinica : IReloj
{
    private static readonly TimeZoneInfo ZonaClinica = ObtenerZona();

    public DateTime AhoraEnClinica => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaClinica);

    private static TimeZoneInfo ObtenerZona()
    {
        // Id IANA (Linux/macOS y Windows con ICU); el id de Windows queda como respaldo.
        foreach (var id in new[] { "America/Argentina/Buenos_Aires", "Argentina Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zona))
                return zona;
        }

        return TimeZoneInfo.CreateCustomTimeZone("Argentina", TimeSpan.FromHours(-3), "Argentina", "Argentina");
    }
}

using Clinica.Dominio.HistoriaClinica;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.HistoriaClinica.Datos;

/// <summary>
/// Nomenclador de ejemplo SOLO para desarrollo. Los códigos y precios son ilustrativos.
/// Es idempotente: si ya hay prestaciones (incluidas las dadas de baja), no hace nada.
/// </summary>
public static class NomencladorSeeder
{
    public static async Task SeedAsync(ClinicaDbContext db, CancellationToken ct = default)
    {
        if (await db.Prestaciones.IgnoreQueryFilters().AnyAsync(ct))
            return;

        db.Prestaciones.AddRange(Prestaciones());
        await db.SaveChangesAsync(ct);
    }

    public static IReadOnlyList<Prestacion> Prestaciones() =>
    [
        new("01.01", "Consulta y diagnóstico", CategoriaPrestacion.Consulta, 30, 15000m),
        new("01.04", "Consulta de urgencia", CategoriaPrestacion.Consulta, 30, 20000m),
        new("02.01", "Obturación simple con amalgama", CategoriaPrestacion.Operatoria, 45, 28000m),
        new("02.08", "Restauración con resina compuesta", CategoriaPrestacion.Operatoria, 60, 35000m),
        new("03.01", "Tratamiento de conducto unirradicular", CategoriaPrestacion.Endodoncia, 90, 75000m, true),
        new("03.02", "Tratamiento de conducto multirradicular", CategoriaPrestacion.Endodoncia, 120, 110000m, true),
        new("04.01", "Prótesis parcial removible acrílica", CategoriaPrestacion.Protesis, 60, 180000m, true),
        new("04.05", "Corona de porcelana sobre metal", CategoriaPrestacion.Protesis, 60, 220000m, true),
        new("05.01", "Tartrectomía y cepillado mecánico", CategoriaPrestacion.Periodoncia, 45, 25000m),
        new("05.02", "Raspaje y alisado radicular por cuadrante", CategoriaPrestacion.Periodoncia, 60, 40000m),
        new("07.01", "Topicación con flúor", CategoriaPrestacion.Prevencion, 20, 12000m),
        new("07.02", "Sellador de fosas y fisuras por pieza", CategoriaPrestacion.Prevencion, 20, 14000m),
        new("08.01", "Radiografía periapical", CategoriaPrestacion.Radiologia, 15, 8000m),
        new("08.05", "Radiografía panorámica", CategoriaPrestacion.Radiologia, 15, 22000m),
        new("09.01", "Tratamiento en dientes temporarios", CategoriaPrestacion.Odontopediatria, 45, 26000m),
        new("10.01", "Extracción simple", CategoriaPrestacion.Cirugia, 30, 30000m),
        new("10.04", "Extracción de pieza retenida", CategoriaPrestacion.Cirugia, 90, 95000m, true),
        new("11.01", "Estudio y diagnóstico de ortodoncia", CategoriaPrestacion.Ortodoncia, 45, 45000m),
    ];
}

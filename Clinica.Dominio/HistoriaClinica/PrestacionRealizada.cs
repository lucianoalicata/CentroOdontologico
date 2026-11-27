using Clinica.Dominio.Comun;

namespace Clinica.Dominio.HistoriaClinica;

/// <summary>
/// Prestación efectivamente realizada en una entrada de la historia clínica.
/// Guarda código, título y precio tal como se informaron en ese momento
/// (no referencia al catálogo), para que un cambio posterior de precio no altere el historial.
/// </summary>
public class PrestacionRealizada : Entidad
{
    public const int LongitudMaximaCodigo = 30;
    public const int LongitudMaximaTitulo = 200;

    public int EntradaHistoriaClinicaId { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Titulo { get; private set; } = null!;
    public decimal PrecioUnitario { get; private set; }
    public int Cantidad { get; private set; }

    /// <summary>Pieza dental en notación FDI (11-48 permanentes, 51-85 temporales). Opcional.</summary>
    public int? PiezaDental { get; private set; }

    /// <summary>Meses de garantía del tratamiento. Null si no tiene garantía.</summary>
    public int? MesesGarantia { get; private set; }

    public decimal Subtotal => PrecioUnitario * Cantidad;

    protected PrestacionRealizada() { }

    public PrestacionRealizada(
        string codigo,
        string titulo,
        decimal precioUnitario,
        int cantidad = 1,
        int? piezaDental = null,
        int? mesesGarantia = null)
    {
        Codigo = ValidarLongitud(Guard.NoVacio(codigo, "Código"), LongitudMaximaCodigo, "El código");
        Titulo = ValidarLongitud(Guard.NoVacio(titulo, "Título"), LongitudMaximaTitulo, "El título");
        PrecioUnitario = Guard.NoNegativo(precioUnitario, "Precio");
        Cantidad = ValidarCantidad(cantidad);
        PiezaDental = ValidarPieza(piezaDental);
        MesesGarantia = ValidarGarantia(mesesGarantia);
    }

    private static string ValidarLongitud(string valor, int maximo, string campo)
    {
        if (valor.Length > maximo)
            throw new DomainException($"{campo} no puede superar los {maximo} caracteres.");
        return valor;
    }

    private static int ValidarCantidad(int cantidad)
    {
        if (cantidad < 1)
            throw new DomainException("La cantidad debe ser al menos 1.");
        return cantidad;
    }

    private static int? ValidarPieza(int? pieza)
    {
        if (pieza is null) return null;

        var cuadrante = pieza.Value / 10;
        var diente = pieza.Value % 10;

        var valida = cuadrante switch
        {
            >= 1 and <= 4 => diente is >= 1 and <= 8,   // dentición permanente
            >= 5 and <= 8 => diente is >= 1 and <= 5,   // dentición temporal
            _ => false
        };

        if (!valida)
            throw new DomainException("La pieza dental no es válida (notación FDI).");
        return pieza;
    }

    private static int? ValidarGarantia(int? meses)
    {
        if (meses is null) return null;
        if (meses <= 0)
            throw new DomainException("Los meses de garantía deben ser mayores a cero.");
        return meses;
    }
}

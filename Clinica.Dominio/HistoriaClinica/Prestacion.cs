using Clinica.Dominio.Comun;

namespace Clinica.Dominio.HistoriaClinica;

/// <summary>
/// Prestación del catálogo (nomenclador) de la clínica.
/// Al registrarse en una entrada de la historia clínica se copia como <see cref="PrestacionRealizada"/>,
/// por lo que editar el catálogo no altera el historial.
/// </summary>
public class Prestacion : EntidadConBorradoLogico
{
    public const int LongitudMaximaCodigo = PrestacionRealizada.LongitudMaximaCodigo;
    public const int LongitudMaximaTitulo = PrestacionRealizada.LongitudMaximaTitulo;
    public const int DuracionMaximaMinutos = 480;

    public string Codigo { get; private set; } = null!;
    public string Titulo { get; private set; } = null!;
    public CategoriaPrestacion Categoria { get; private set; }
    public int DuracionEstimadaMinutos { get; private set; }
    public decimal PrecioParticular { get; private set; }
    public bool RequiereAutorizacionPrevia { get; private set; }

    protected Prestacion() { }

    public Prestacion(
        string codigo,
        string titulo,
        CategoriaPrestacion categoria,
        int duracionEstimadaMinutos,
        decimal precioParticular,
        bool requiereAutorizacionPrevia = false)
    {
        Codigo = ValidarLongitud(Guard.NoVacio(codigo, "Código"), LongitudMaximaCodigo, "El código");
        AsignarDatos(titulo, categoria, duracionEstimadaMinutos, precioParticular, requiereAutorizacionPrevia);
    }

    /// <summary>Modifica los datos editables. El código identifica a la prestación y no cambia.</summary>
    public void Actualizar(
        string titulo,
        CategoriaPrestacion categoria,
        int duracionEstimadaMinutos,
        decimal precioParticular,
        bool requiereAutorizacionPrevia)
    {
        AsignarDatos(titulo, categoria, duracionEstimadaMinutos, precioParticular, requiereAutorizacionPrevia);
    }

    public void CambiarPrecio(decimal precioParticular)
    {
        PrecioParticular = Guard.NoNegativo(precioParticular, "Precio particular");
    }

    private void AsignarDatos(
        string titulo,
        CategoriaPrestacion categoria,
        int duracionEstimadaMinutos,
        decimal precioParticular,
        bool requiereAutorizacionPrevia)
    {
        Titulo = ValidarLongitud(Guard.NoVacio(titulo, "Título"), LongitudMaximaTitulo, "El título");
        Categoria = ValidarCategoria(categoria);
        DuracionEstimadaMinutos = ValidarDuracion(duracionEstimadaMinutos);
        PrecioParticular = Guard.NoNegativo(precioParticular, "Precio particular");
        RequiereAutorizacionPrevia = requiereAutorizacionPrevia;
    }

    private static string ValidarLongitud(string valor, int maximo, string campo)
    {
        if (valor.Length > maximo)
            throw new DomainException($"{campo} no puede superar los {maximo} caracteres.");
        return valor;
    }

    private static CategoriaPrestacion ValidarCategoria(CategoriaPrestacion categoria)
    {
        if (!Enum.IsDefined(categoria))
            throw new DomainException("La categoría no es válida.");
        return categoria;
    }

    private static int ValidarDuracion(int minutos)
    {
        if (minutos <= 0)
            throw new DomainException("La duración estimada debe ser mayor a cero.");
        if (minutos > DuracionMaximaMinutos)
            throw new DomainException($"La duración estimada no puede superar los {DuracionMaximaMinutos} minutos.");
        return minutos;
    }
}

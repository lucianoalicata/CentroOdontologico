namespace Clinica.Dominio.Comun;

public record ResultadoPaginado<T>(
    IReadOnlyList<T> Items,
    int TotalItems,
    int PageIndex,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
namespace Clinica.Dominio.Comun;

public static class Guard
{
    public static string NoVacio(string? valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException($"{campo} es obligatorio.");
        return valor.Trim();
    }

    public static int IdValido(int valor, string campo)
    {
        if (valor <= 0)
            throw new DomainException($"{campo} no es válido.");
        return valor;
    }

    public static decimal NoNegativo(decimal valor, string campo)
    {
        if (valor < 0)
            throw new DomainException($"{campo} no puede ser negativo.");
        return valor;
    }
}
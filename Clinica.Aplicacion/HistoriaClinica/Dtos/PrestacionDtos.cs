using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Aplicacion.HistoriaClinica.Dtos;

public record PrestacionDto(
    int Id,
    string Codigo,
    string Titulo,
    CategoriaPrestacion Categoria,
    int DuracionEstimadaMinutos,
    decimal PrecioParticular,
    bool RequiereAutorizacionPrevia,
    bool Activo);

public record CrearPrestacionDto(
    string Codigo,
    string Titulo,
    CategoriaPrestacion Categoria,
    int DuracionEstimadaMinutos,
    decimal PrecioParticular,
    bool RequiereAutorizacionPrevia);

public record ActualizarPrestacionDto(
    string Titulo,
    CategoriaPrestacion Categoria,
    int DuracionEstimadaMinutos,
    decimal PrecioParticular,
    bool RequiereAutorizacionPrevia);

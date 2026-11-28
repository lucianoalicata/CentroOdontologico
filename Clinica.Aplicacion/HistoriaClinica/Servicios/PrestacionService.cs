using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Aplicacion.HistoriaClinica.Servicios;

public class PrestacionService : IPrestacionService
{
    private readonly IPrestacionRepository _prestaciones;
    private readonly IUnitOfWork _unitOfWork;

    public PrestacionService(IPrestacionRepository prestaciones, IUnitOfWork unitOfWork)
    {
        _prestaciones = prestaciones;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PrestacionDto>> ListarActivasAsync(
        string? filtro, CategoriaPrestacion? categoria, CancellationToken ct = default)
    {
        var lista = await _prestaciones.ListarAsync(filtro, categoria, incluirInactivas: false, ct);
        return lista.Select(MapearADto).ToList();
    }

    public async Task<IReadOnlyList<PrestacionDto>> ListarTodasAsync(
        string? filtro, CategoriaPrestacion? categoria, CancellationToken ct = default)
    {
        var lista = await _prestaciones.ListarAsync(filtro, categoria, incluirInactivas: true, ct);
        return lista.Select(MapearADto).ToList();
    }

    public async Task<PrestacionDto> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var prestacion = await _prestaciones.ObtenerPorIdIncluyendoInactivasAsync(id, ct)
            ?? throw new NoEncontradoException("la prestación");
        return MapearADto(prestacion);
    }

    /// <summary>
    /// Da de alta una prestación. Si el código pertenece a una prestación dada de baja,
    /// se la reactiva con los datos nuevos en lugar de crear un duplicado.
    /// </summary>
    public async Task<PrestacionDto> CrearAsync(CrearPrestacionDto dto, CancellationToken ct = default)
    {
        var codigo = Guard.NoVacio(dto.Codigo, "Código");
        var existente = await _prestaciones.ObtenerPorCodigoIncluyendoInactivasAsync(codigo, ct);

        if (existente is not null)
        {
            if (existente.Activo)
                throw new DomainException($"Ya existe una prestación con el código {codigo}.");

            existente.Actualizar(dto.Titulo, dto.Categoria, dto.DuracionEstimadaMinutos,
                dto.PrecioParticular, dto.RequiereAutorizacionPrevia);
            existente.Reactivar();
            await _unitOfWork.GuardarCambiosAsync(ct);
            return MapearADto(existente);
        }

        var prestacion = new Prestacion(codigo, dto.Titulo, dto.Categoria, dto.DuracionEstimadaMinutos,
            dto.PrecioParticular, dto.RequiereAutorizacionPrevia);

        await _prestaciones.AgregarAsync(prestacion, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);
        return MapearADto(prestacion);
    }

    public async Task<PrestacionDto> ActualizarAsync(int id, ActualizarPrestacionDto dto, CancellationToken ct = default)
    {
        var prestacion = await _prestaciones.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("la prestación");

        prestacion.Actualizar(dto.Titulo, dto.Categoria, dto.DuracionEstimadaMinutos,
            dto.PrecioParticular, dto.RequiereAutorizacionPrevia);
        await _unitOfWork.GuardarCambiosAsync(ct);
        return MapearADto(prestacion);
    }

    public async Task DarDeBajaAsync(int id, CancellationToken ct = default)
    {
        var prestacion = await _prestaciones.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("la prestación");

        prestacion.Desactivar();
        await _unitOfWork.GuardarCambiosAsync(ct);
    }

    public async Task<PrestacionDto> ReactivarAsync(int id, CancellationToken ct = default)
    {
        var prestacion = await _prestaciones.ObtenerPorIdIncluyendoInactivasAsync(id, ct)
            ?? throw new NoEncontradoException("la prestación");

        prestacion.Reactivar();
        await _unitOfWork.GuardarCambiosAsync(ct);
        return MapearADto(prestacion);
    }

    public static PrestacionDto MapearADto(Prestacion p) => new(
        p.Id,
        p.Codigo,
        p.Titulo,
        p.Categoria,
        p.DuracionEstimadaMinutos,
        p.PrecioParticular,
        p.RequiereAutorizacionPrevia,
        p.Activo);
}

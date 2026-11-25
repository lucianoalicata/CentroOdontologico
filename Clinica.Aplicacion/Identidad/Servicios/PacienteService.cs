// Clinica.Aplicacion/Identidad/Servicios/PacienteService.cs
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;

namespace Clinica.Aplicacion.Identidad.Servicios;

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _pacientes;
    private readonly IUnitOfWork _unitOfWork;

    public PacienteService(IPacienteRepository pacientes, IUnitOfWork unitOfWork)
    {
        _pacientes = pacientes;
        _unitOfWork = unitOfWork;
    }

    public async Task<PacienteDto> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var paciente = await _pacientes.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("paciente");

        return RegistrarPacienteUseCase.MapearADto(paciente);
    }

    public async Task<ResultadoPaginado<PacienteDto>> BuscarPaginadoAsync(
        string? filtro, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var resultado = await _pacientes.BuscarPaginadoAsync(filtro, pageIndex, pageSize, ct);
        var itemsDto = resultado.Items.Select(RegistrarPacienteUseCase.MapearADto).ToList();

        return new ResultadoPaginado<PacienteDto>(
            itemsDto,
            resultado.TotalItems,
            resultado.PageIndex,
            resultado.PageSize);
    }

    public async Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken ct = default)
    {
        var paciente = await _pacientes.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("paciente");

        paciente.ActualizarDatos(dto.Nombre, dto.Apellido, dto.Telefono, dto.FechaNacimiento, dto.Domicilio);
        await _unitOfWork.GuardarCambiosAsync(ct);

        return RegistrarPacienteUseCase.MapearADto(paciente);
    }

    public async Task DarDeBajaAsync(int id, CancellationToken ct = default)
    {
        var paciente = await _pacientes.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("paciente");

        paciente.Desactivar();
        await _unitOfWork.GuardarCambiosAsync(ct);
    }

    public async Task<PacienteDto> ReactivarAsync(int id, CancellationToken ct = default)
    {
        var paciente = await _pacientes.ObtenerPorIdIncluyendoInactivosAsync(id, ct)
            ?? throw new NoEncontradoException("paciente");

        paciente.Reactivar();
        await _unitOfWork.GuardarCambiosAsync(ct);

        return RegistrarPacienteUseCase.MapearADto(paciente);
    }
}
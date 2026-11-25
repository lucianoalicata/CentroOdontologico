using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;

namespace Clinica.Aplicacion.Identidad.Servicios;

public class RegistrarPacienteUseCase
{
    private readonly IPacienteRepository _pacientes;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarPacienteUseCase(IPacienteRepository pacientes, IUnitOfWork unitOfWork)
    {
        _pacientes = pacientes;
        _unitOfWork = unitOfWork;
    }

    public async Task<PacienteDto> EjecutarAsync(RegistrarPacienteDto dto, CancellationToken ct = default)
    {
        var nuevo = new PacientePerfil(
            dto.Nombre,
            dto.Apellido,
            dto.Dni,
            dto.Telefono,
            dto.FechaNacimiento,
            dto.Domicilio);

        var existente = await _pacientes.ObtenerPorDniIncluyendoInactivosAsync(nuevo.Dni, ct);

        if (existente is not null)
        {
            if (existente.Activo)
                throw new DomainException($"Ya existe un paciente activo registrado con el DNI {nuevo.Dni}.");

            existente.Reactivar();
            existente.ActualizarDatos(
                dto.Nombre,
                dto.Apellido,
                dto.Telefono,
                dto.FechaNacimiento,
                dto.Domicilio);

            await _unitOfWork.GuardarCambiosAsync(ct);
            return MapearADto(existente);
        }

        await _pacientes.AgregarAsync(nuevo, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);

        return MapearADto(nuevo);
    }

    internal static PacienteDto MapearADto(PacientePerfil p) =>
        new(p.Id, p.Nombre, p.Apellido, p.Dni, p.Telefono, p.FechaNacimiento, p.Domicilio, p.TieneCuentaWeb, p.Activo);
}
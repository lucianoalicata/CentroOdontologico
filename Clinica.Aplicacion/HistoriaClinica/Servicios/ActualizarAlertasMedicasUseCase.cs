using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Aplicacion.HistoriaClinica.Puertos;
using Clinica.Aplicacion.Identidad;
using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Aplicacion.HistoriaClinica.Servicios;

/// <summary>
/// Edita las alertas médicas de la historia (alergias, medicación, etc.). Solo un odontólogo.
/// Si el paciente no tiene historia, la crea con esas alertas. Texto vacío = sin alertas.
/// </summary>
public class ActualizarAlertasMedicasUseCase
{
    private readonly IHistoriaClinicaRepository _historias;
    private readonly IConsultaPacientes _pacientes;
    private readonly IUsuarioActual _usuario;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarAlertasMedicasUseCase(
        IHistoriaClinicaRepository historias,
        IConsultaPacientes pacientes,
        IUsuarioActual usuario,
        IUnitOfWork unitOfWork)
    {
        _historias = historias;
        _pacientes = pacientes;
        _usuario = usuario;
        _unitOfWork = unitOfWork;
    }

    public async Task<HistoriaClinicaDto> EjecutarAsync(int pacienteId, ActualizarAlertasDto dto, CancellationToken ct = default)
    {
        PermisosHistoriaClinica.ExigirOdontologo(_usuario);
        Guard.IdValido(pacienteId, "Paciente");

        if (!await _pacientes.ExistePacienteActivoAsync(pacienteId, ct))
            throw new NoEncontradoException("el paciente");

        var historia = await _historias.ObtenerPorPacienteIdAsync(pacienteId, ct);
        var esNueva = historia is null;
        historia ??= new HistoriaClinicaPaciente(pacienteId);

        historia.ActualizarAlertasMedicas(dto.AlertasMedicas);

        if (esNueva)
            await _historias.AgregarAsync(historia, ct);

        await _unitOfWork.GuardarCambiosAsync(ct);
        return HistoriaClinicaMapeo.HistoriaADto(historia);
    }
}
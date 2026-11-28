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
/// Devuelve la historia clínica del paciente; si es el primer acceso, la crea.
/// Pueden usarlo el odontólogo y el administrador.
/// </summary>
public class ObtenerHistoriaClinicaUseCase
{
    private readonly IHistoriaClinicaRepository _historias;
    private readonly IConsultaPacientes _pacientes;
    private readonly IUsuarioActual _usuario;
    private readonly IUnitOfWork _unitOfWork;

    public ObtenerHistoriaClinicaUseCase(
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

    public async Task<HistoriaClinicaDto> EjecutarAsync(int pacienteId, CancellationToken ct = default)
    {
        PermisosHistoriaClinica.ExigirConsulta(_usuario);
        Guard.IdValido(pacienteId, "Paciente");

        if (!await _pacientes.ExistePacienteActivoAsync(pacienteId, ct))
            throw new NoEncontradoException("el paciente");

        var historia = await _historias.ObtenerPorPacienteIdAsync(pacienteId, ct);
        if (historia is null)
        {
            historia = new HistoriaClinicaPaciente(pacienteId);
            await _historias.AgregarAsync(historia, ct);
            await _unitOfWork.GuardarCambiosAsync(ct);
        }

        return HistoriaClinicaMapeo.HistoriaADto(historia);
    }
}
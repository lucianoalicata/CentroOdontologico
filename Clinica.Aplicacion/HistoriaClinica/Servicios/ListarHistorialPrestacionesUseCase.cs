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
/// Historial de prestaciones del paciente: todas las prestaciones realizadas, en orden
/// cronológico, indicando si su entrada fue corregida por otra posterior.
/// </summary>
public class ListarHistorialPrestacionesUseCase
{
    private readonly IHistoriaClinicaRepository _historias;
    private readonly IConsultaPacientes _pacientes;
    private readonly IUsuarioActual _usuario;

    public ListarHistorialPrestacionesUseCase(
        IHistoriaClinicaRepository historias,
        IConsultaPacientes pacientes,
        IUsuarioActual usuario)
    {
        _historias = historias;
        _pacientes = pacientes;
        _usuario = usuario;
    }

    public async Task<IReadOnlyList<PrestacionHistorialDto>> EjecutarAsync(int pacienteId, CancellationToken ct = default)
    {
        PermisosHistoriaClinica.ExigirConsulta(_usuario);
        Guard.IdValido(pacienteId, "Paciente");

        if (!await _pacientes.ExistePacienteActivoAsync(pacienteId, ct))
            throw new NoEncontradoException("el paciente");

        var historia = await _historias.ObtenerPorPacienteIdAsync(pacienteId, ct);
        if (historia is null)
            return Array.Empty<PrestacionHistorialDto>();

        var entradas = await _historias.ListarEntradasAsync(historia.Id, ct);
        return HistoriaClinicaMapeo.HistorialADto(entradas);
    }
}
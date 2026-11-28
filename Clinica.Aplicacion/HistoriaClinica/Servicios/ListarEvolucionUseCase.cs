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
/// Evolución clínica del paciente: sus entradas de la más antigua a la más reciente.
/// Si todavía no tiene historia devuelve una lista vacía (no la crea).
/// </summary>
public class ListarEvolucionUseCase
{
    private readonly IHistoriaClinicaRepository _historias;
    private readonly IConsultaPacientes _pacientes;
    private readonly IUsuarioActual _usuario;

    public ListarEvolucionUseCase(
        IHistoriaClinicaRepository historias,
        IConsultaPacientes pacientes,
        IUsuarioActual usuario)
    {
        _historias = historias;
        _pacientes = pacientes;
        _usuario = usuario;
    }

    public async Task<IReadOnlyList<EntradaHistoriaClinicaDto>> EjecutarAsync(int pacienteId, CancellationToken ct = default)
    {
        PermisosHistoriaClinica.ExigirConsulta(_usuario);
        Guard.IdValido(pacienteId, "Paciente");

        if (!await _pacientes.ExistePacienteActivoAsync(pacienteId, ct))
            throw new NoEncontradoException("el paciente");

        var historia = await _historias.ObtenerPorPacienteIdAsync(pacienteId, ct);
        if (historia is null)
            return Array.Empty<EntradaHistoriaClinicaDto>();

        var entradas = await _historias.ListarEntradasAsync(historia.Id, ct);
        return HistoriaClinicaMapeo.EntradasADto(entradas);
    }
}
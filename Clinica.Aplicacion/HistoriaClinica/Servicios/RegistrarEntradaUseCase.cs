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
/// Registra una entrada de la historia clínica con sus prestaciones realizadas.
/// Solo un odontólogo; el autor sale del usuario logueado. Si el paciente no tiene
/// historia, se crea en este mismo guardado. Las prestaciones se copian del catálogo
/// (código, título y precio de ese momento).
/// </summary>
public class RegistrarEntradaUseCase
{
    private readonly IHistoriaClinicaRepository _historias;
    private readonly IConsultaPacientes _pacientes;
    private readonly IPrestacionRepository _prestaciones;
    private readonly IUsuarioActual _usuario;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarEntradaUseCase(
        IHistoriaClinicaRepository historias,
        IConsultaPacientes pacientes,
        IPrestacionRepository prestaciones,
        IUsuarioActual usuario,
        IUnitOfWork unitOfWork)
    {
        _historias = historias;
        _pacientes = pacientes;
        _prestaciones = prestaciones;
        _usuario = usuario;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntradaHistoriaClinicaDto> EjecutarAsync(RegistrarEntradaDto dto, CancellationToken ct = default)
    {
        var odontologoId = PermisosHistoriaClinica.ExigirOdontologo(_usuario);
        Guard.IdValido(dto.PacienteId, "Paciente");

        if (!await _pacientes.ExistePacienteActivoAsync(dto.PacienteId, ct))
            throw new NoEncontradoException("el paciente");

        var realizadas = await ArmarPrestacionesAsync(dto.Prestaciones, ct);

        // Con las entradas cargadas, porque la historia valida que la entrada a corregir le pertenezca
        var historia = await _historias.ObtenerPorPacienteIdConEntradasAsync(dto.PacienteId, ct);
        var esNueva = historia is null;
        historia ??= new HistoriaClinicaPaciente(dto.PacienteId);

        var entrada = historia.AgregarEntrada(odontologoId, dto.Comentario, realizadas, dto.CorrigeEntradaId);

        if (esNueva)
            await _historias.AgregarAsync(historia, ct);

        await _unitOfWork.GuardarCambiosAsync(ct);
        return HistoriaClinicaMapeo.EntradaADto(entrada);
    }

    private async Task<List<PrestacionRealizada>> ArmarPrestacionesAsync(
        IReadOnlyList<RegistrarPrestacionDto>? pedidas, CancellationToken ct)
    {
        var resultado = new List<PrestacionRealizada>();

        foreach (var pedida in pedidas ?? Array.Empty<RegistrarPrestacionDto>())
        {
            Guard.IdValido(pedida.PrestacionId, "Prestación");

            // Solo prestaciones activas del catálogo
            var catalogo = await _prestaciones.ObtenerPorIdAsync(pedida.PrestacionId, ct)
                ?? throw new NoEncontradoException("la prestación");

            resultado.Add(new PrestacionRealizada(
                catalogo.Codigo,
                catalogo.Titulo,
                catalogo.PrecioParticular,
                pedida.Cantidad,
                pedida.PiezaDental,
                pedida.MesesGarantia));
        }

        return resultado;
    }
}
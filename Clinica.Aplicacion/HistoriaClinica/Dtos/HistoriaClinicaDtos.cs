using System;
using System.Collections.Generic;
using System.Text;

namespace Clinica.Aplicacion.HistoriaClinica.Dtos;

public record HistoriaClinicaDto(int Id, int PacienteId, string? AlertasMedicas);

public record PrestacionRealizadaDto(
    int Id,
    string Codigo,
    string Titulo,
    decimal PrecioUnitario,
    int Cantidad,
    decimal Subtotal,
    int? PiezaDental,
    int? MesesGarantia);

public record EntradaHistoriaClinicaDto(
    int Id,
    int HistoriaClinicaId,
    int OdontologoId,
    string? Comentario,
    int? CorrigeEntradaId,
    bool FueCorregida,
    DateTime FechaCreacion,
    IReadOnlyList<PrestacionRealizadaDto> Prestaciones);

/// <summary>Una prestación realizada, con los datos de su entrada (para el historial de prestaciones).</summary>
public record PrestacionHistorialDto(
    int EntradaId,
    DateTime Fecha,
    int OdontologoId,
    int PrestacionRealizadaId,
    string Codigo,
    string Titulo,
    decimal PrecioUnitario,
    int Cantidad,
    decimal Subtotal,
    int? PiezaDental,
    int? MesesGarantia,
    bool EntradaCorregida);

/// <summary>Prestación del catálogo que se realizó en la atención.</summary>
public record RegistrarPrestacionDto(
    int PrestacionId,
    int Cantidad = 1,
    int? PiezaDental = null,
    int? MesesGarantia = null);

public record RegistrarEntradaDto(
    int PacienteId,
    string? Comentario,
    int? CorrigeEntradaId,
    IReadOnlyList<RegistrarPrestacionDto>? Prestaciones);

public record ActualizarAlertasDto(string? AlertasMedicas);
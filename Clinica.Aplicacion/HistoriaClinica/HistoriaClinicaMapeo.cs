using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.HistoriaClinica.Dtos;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Aplicacion.HistoriaClinica;

/// <summary>Conversiones compartidas por los casos de uso de la historia clínica.</summary>
public static class HistoriaClinicaMapeo
{
    public static HistoriaClinicaDto HistoriaADto(HistoriaClinicaPaciente h) =>
        new(h.Id, h.PacienteId, h.AlertasMedicas);

    public static PrestacionRealizadaDto PrestacionADto(PrestacionRealizada p) => new(
        p.Id, p.Codigo, p.Titulo, p.PrecioUnitario, p.Cantidad, p.Subtotal, p.PiezaDental, p.MesesGarantia);

    public static EntradaHistoriaClinicaDto EntradaADto(EntradaHistoriaClinica e, bool fueCorregida = false) => new(
        e.Id,
        e.HistoriaClinicaId,
        e.OdontologoId,
        e.Comentario,
        e.CorrigeEntradaId,
        fueCorregida,
        e.FechaCreacion,
        e.Prestaciones.Select(PrestacionADto).ToList());

    public static IReadOnlyList<EntradaHistoriaClinicaDto> EntradasADto(IReadOnlyList<EntradaHistoriaClinica> entradas)
    {
        var corregidas = IdsCorregidos(entradas);
        return entradas.Select(e => EntradaADto(e, corregidas.Contains(e.Id))).ToList();
    }

    public static IReadOnlyList<PrestacionHistorialDto> HistorialADto(IReadOnlyList<EntradaHistoriaClinica> entradas)
    {
        var corregidas = IdsCorregidos(entradas);
        return entradas
            .SelectMany(e => e.Prestaciones.Select(p => new PrestacionHistorialDto(
                e.Id, e.FechaCreacion, e.OdontologoId, p.Id, p.Codigo, p.Titulo,
                p.PrecioUnitario, p.Cantidad, p.Subtotal, p.PiezaDental, p.MesesGarantia,
                corregidas.Contains(e.Id))))
            .ToList();
    }

    private static HashSet<int> IdsCorregidos(IEnumerable<EntradaHistoriaClinica> entradas) =>
        entradas.Where(e => e.CorrigeEntradaId.HasValue).Select(e => e.CorrigeEntradaId!.Value).ToHashSet();
}
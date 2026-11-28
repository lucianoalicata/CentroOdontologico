using System;
using System.Collections.Generic;
using System.Text;

namespace Clinica.Aplicacion.HistoriaClinica.Puertos;

/// <summary>
/// Lectura de datos de Identidad que necesita la historia clínica.
/// La historia referencia al paciente solo por Id.
/// </summary>
public interface IConsultaPacientes
{
    Task<bool> ExistePacienteActivoAsync(int pacienteId, CancellationToken ct = default);
}
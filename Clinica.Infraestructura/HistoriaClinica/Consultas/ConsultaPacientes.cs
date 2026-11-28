using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.HistoriaClinica.Puertos;
using Clinica.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.HistoriaClinica.Consultas;

public class ConsultaPacientes : IConsultaPacientes
{
    private readonly ClinicaDbContext _db;

    public ConsultaPacientes(ClinicaDbContext db)
    {
        _db = db;
    }

    // El filtro global de borrado lógico deja afuera a los pacientes dados de baja
    public Task<bool> ExistePacienteActivoAsync(int pacienteId, CancellationToken ct = default) =>
        _db.Pacientes.AnyAsync(p => p.Id == pacienteId, ct);
}
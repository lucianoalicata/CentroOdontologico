using Clinica.Dominio.Comun;

namespace Clinica.Dominio.HistoriaClinica;

/// <summary>
/// Historia clínica de un paciente (una por paciente). Raíz del módulo: las entradas
/// se agregan únicamente mediante <see cref="AgregarEntrada"/>.
/// Se llama HistoriaClinicaPaciente y no HistoriaClinica porque una clase con el mismo nombre
/// que el namespace del módulo no se puede usar desde las otras capas (error CS0118).
/// </summary>
public class HistoriaClinicaPaciente : Entidad
{
    public const int LongitudMaximaAlertas = 1000;

    private readonly List<EntradaHistoriaClinica> _entradas = new();

    /// <summary>Referencia a PacientePerfil.Id (solo por Id, sin navegación entre módulos).</summary>
    public int PacienteId { get; private set; }
    public string? AlertasMedicas { get; private set; }
    public IReadOnlyCollection<EntradaHistoriaClinica> Entradas => _entradas.AsReadOnly();

    protected HistoriaClinicaPaciente() { }

    public HistoriaClinicaPaciente(int pacienteId)
    {
        PacienteId = Guard.IdValido(pacienteId, "Paciente");
    }

    public void ActualizarAlertasMedicas(string? alertas)
    {
        if (string.IsNullOrWhiteSpace(alertas))
        {
            AlertasMedicas = null;
            return;
        }

        var limpio = alertas.Trim();
        if (limpio.Length > LongitudMaximaAlertas)
            throw new DomainException($"Las alertas médicas no pueden superar los {LongitudMaximaAlertas} caracteres.");
        AlertasMedicas = limpio;
    }

    /// <summary>
    /// Agrega una entrada nueva. Si corrige otra, esa entrada debe pertenecer a esta historia
    /// (el caso de uso tiene que cargar la historia con sus entradas).
    /// </summary>
    public EntradaHistoriaClinica AgregarEntrada(
        int odontologoId,
        string? comentario,
        IEnumerable<PrestacionRealizada>? prestaciones = null,
        int? corrigeEntradaId = null)
    {
        if (corrigeEntradaId.HasValue && _entradas.All(e => e.Id != corrigeEntradaId.Value))
            throw new DomainException("La entrada que se quiere corregir no pertenece a esta historia clínica.");

        var entrada = new EntradaHistoriaClinica(odontologoId, comentario, prestaciones, corrigeEntradaId);
        _entradas.Add(entrada);
        return entrada;
    }
}

using Clinica.Dominio.Comun;

namespace Clinica.Dominio.HistoriaClinica;

/// <summary>
/// Registro de una atención. Es inmutable: una vez creada no se modifica ni se elimina.
/// Un error se corrige con una entrada nueva que referencia a la anterior (CorrigeEntradaId).
/// </summary>
public class EntradaHistoriaClinica : Entidad
{
    public const int LongitudMaximaComentario = 2000;

    private readonly List<PrestacionRealizada> _prestaciones = new();

    public int HistoriaClinicaId { get; private set; }
    public int OdontologoId { get; private set; }
    public string? Comentario { get; private set; }
    public int? CorrigeEntradaId { get; private set; }
    public IReadOnlyCollection<PrestacionRealizada> Prestaciones => _prestaciones.AsReadOnly();

    protected EntradaHistoriaClinica() { }

    public EntradaHistoriaClinica(
        int odontologoId,
        string? comentario,
        IEnumerable<PrestacionRealizada>? prestaciones = null,
        int? corrigeEntradaId = null)
    {
        OdontologoId = Guard.IdValido(odontologoId, "Odontólogo");
        Comentario = NormalizarComentario(comentario);
        CorrigeEntradaId = corrigeEntradaId.HasValue
            ? Guard.IdValido(corrigeEntradaId.Value, "Entrada a corregir")
            : null;

        if (prestaciones is not null)
            _prestaciones.AddRange(prestaciones);

        if (Comentario is null && _prestaciones.Count == 0)
            throw new DomainException("La entrada debe tener un comentario o al menos una prestación realizada.");
    }

    private static string? NormalizarComentario(string? comentario)
    {
        if (string.IsNullOrWhiteSpace(comentario)) return null;

        var limpio = comentario.Trim();
        if (limpio.Length > LongitudMaximaComentario)
            throw new DomainException($"El comentario no puede superar los {LongitudMaximaComentario} caracteres.");
        return limpio;
    }
}

using Clinica.Dominio.Comun;

namespace Clinica.Dominio.Identidad;

// El paciente existe como persona clínica aunque no tenga cuenta web.
// UsuarioId es opcional: se completa cuando el paciente accede al portal.
public class PacientePerfil : EntidadConBorradoLogico
{
    public int? UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string Apellido { get; private set; } = null!;
    public string Dni { get; private set; } = null!;
    public string Telefono { get; private set; } = null!;
    public DateOnly FechaNacimiento { get; private set; }

    public bool TieneCuentaWeb => UsuarioId.HasValue;

    protected PacientePerfil() { }

    public PacientePerfil(string nombre, string apellido, string dni, string telefono,
        DateOnly fechaNacimiento, int? usuarioId = null)
    {
        Nombre = Guard.NoVacio(nombre, "Nombre");
        Apellido = Guard.NoVacio(apellido, "Apellido");
        Dni = NormalizarDni(dni);
        Telefono = Guard.NoVacio(telefono, "Teléfono");
        FechaNacimiento = ValidarFechaNacimiento(fechaNacimiento);
        UsuarioId = usuarioId.HasValue ? Guard.IdValido(usuarioId.Value, "UsuarioId") : null;
    }

    public void ActualizarDatos(string nombre, string apellido, string telefono)
    {
        Nombre = Guard.NoVacio(nombre, "Nombre");
        Apellido = Guard.NoVacio(apellido, "Apellido");
        Telefono = Guard.NoVacio(telefono, "Teléfono");
    }

    public void VincularUsuario(int usuarioId)
    {
        if (UsuarioId.HasValue)
            throw new DomainException("El paciente ya tiene una cuenta vinculada.");
        UsuarioId = Guard.IdValido(usuarioId, "UsuarioId");
    }

    private static string NormalizarDni(string dni)
    {
        var limpio = Guard.NoVacio(dni, "DNI").Replace(".", "").Replace(" ", "");
        if (limpio.Length is < 7 or > 8 || !limpio.All(char.IsDigit))
            throw new DomainException("El DNI debe tener 7 u 8 dígitos.");
        return limpio;
    }

    private static DateOnly ValidarFechaNacimiento(DateOnly fecha)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (fecha > hoy)
            throw new DomainException("La fecha de nacimiento no puede ser futura.");
        return fecha;
    }
}
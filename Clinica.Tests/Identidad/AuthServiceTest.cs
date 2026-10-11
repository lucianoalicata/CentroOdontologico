using Clinica.Aplicacion.Identidad.Dtos;
using Clinica.Aplicacion.Identidad.Excepciones;
using Clinica.Aplicacion.Identidad.Puertos;
using Clinica.Aplicacion.Identidad.Servicios;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;

namespace Clinica.Tests.Identidad;

public class AuthServiceTests
{
    private const string Password = "Secreta123";

    private readonly FakeUsuarios _usuarios = new();
    private readonly FakePerfiles _perfiles = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_usuarios, _perfiles, new FakeHasher(), new FakeJwt());
    }

    [Fact]
    public async Task Login_CuandoSonCredencialesCorrectas_EntoncesDevuelveTokenYSesion()
    {
        _usuarios.Agregar(CrearUsuario(id: 7));
        _perfiles.Secretaria = ConId(new SecretariaPerfil(7, "L-100"), 3);

        var respuesta = await _sut.LoginAsync(new LoginDto("ana@clinica.com", Password));

        Assert.Equal("token-falso", respuesta.Token);
        Assert.Equal(7, respuesta.Usuario.Id);
        Assert.Contains("Secretaria", respuesta.Usuario.Roles);
        Assert.Equal(3, respuesta.Usuario.SecretariaId);
        Assert.Null(respuesta.Usuario.PacienteId);
        Assert.Null(respuesta.Usuario.OdontologoId);
    }

    [Fact]
    public async Task Login_ConRolPeroSinPerfil_DevuelveIdDePerfilNulo()
    {
        _usuarios.Agregar(CrearUsuario(id: 7));

        var respuesta = await _sut.LoginAsync(new LoginDto("ana@clinica.com", Password));

        Assert.Null(respuesta.Usuario.SecretariaId);
    }

    [Fact]
    public async Task Login_NormalizaElEmail()
    {
        _usuarios.Agregar(CrearUsuario(id: 7));

        await _sut.LoginAsync(new LoginDto("  ANA@Clinica.com ", Password));

        Assert.Equal("ana@clinica.com", _usuarios.UltimoEmailBuscado);
    }

    [Fact]
    public async Task Login_EmailInexistente_LanzaCredencialesInvalidas()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _sut.LoginAsync(new LoginDto("nadie@clinica.com", Password)));
    }

    [Fact]
    public async Task Login_PasswordIncorrecta_LanzaCredencialesInvalidas()
    {
        _usuarios.Agregar(CrearUsuario(id: 7));

        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _sut.LoginAsync(new LoginDto("ana@clinica.com", "otra-clave")));
    }

    [Fact]
    public async Task Login_UsuarioDadoDeBaja_LanzaCredencialesInvalidas()
    {
        var usuario = CrearUsuario(id: 7);
        usuario.Desactivar();
        _usuarios.Agregar(usuario);

        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _sut.LoginAsync(new LoginDto("ana@clinica.com", Password)));
    }

    [Fact]
    public async Task Login_ConCamposVacios_LanzaCredencialesInvalidas()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _sut.LoginAsync(new LoginDto("", "")));
    }

    [Fact]
    public async Task ObtenerSesion_UsuarioInexistente_LanzaCredencialesInvalidas()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(
            () => _sut.ObtenerSesionAsync(999));
    }

    // ---------- helpers y fakes ----------

    private static Usuario CrearUsuario(int id) =>
        ConId(new Usuario("Ana", "Pérez", "ana@clinica.com", FakeHasher.Prefijo + Password, RolUsuario.Secretaria), id);

    // Id tiene setter protegido: en tests lo asignamos por reflexión
    private static T ConId<T>(T entidad, int id) where T : Entidad
    {
        typeof(Entidad).GetProperty(nameof(Entidad.Id))!.SetValue(entidad, id);
        return entidad;
    }

    private class FakeHasher : IPasswordHasher
    {
        public const string Prefijo = "hash:";
        public string Hashear(string password) => Prefijo + password;
        public bool Verificar(string password, string hash) => hash == Prefijo + password;
    }

    private class FakeJwt : IJwtProvider
    {
        public TokenGenerado Generar(UsuarioSesionDto sesion) =>
            new("token-falso", DateTime.UtcNow.AddHours(1));
    }

    private class FakeUsuarios : IUsuarioRepository
    {
        private readonly List<Usuario> _lista = new();
        public string? UltimoEmailBuscado { get; private set; }

        public void Agregar(Usuario usuario) => _lista.Add(usuario);

        public Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default)
        {
            UltimoEmailBuscado = email;
            return Task.FromResult(_lista.FirstOrDefault(u => u.Email == email));
        }

        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(_lista.FirstOrDefault(u => u.Id == id));
    }

    private class FakePerfiles : IPerfilRepository
    {
        public PacientePerfil? Paciente { get; set; }
        public OdontologoPerfil? Odontologo { get; set; }
        public SecretariaPerfil? Secretaria { get; set; }

        public Task<PacientePerfil?> ObtenerPacientePorUsuarioIdAsync(int usuarioId, CancellationToken ct = default) =>
            Task.FromResult(Paciente);
        public Task<OdontologoPerfil?> ObtenerOdontologoPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default) =>
            Task.FromResult(Odontologo);
        public Task<SecretariaPerfil?> ObtenerSecretariaPorUsuarioIdAsync(int usuarioId, CancellationToken ct = default) =>
            Task.FromResult(Secretaria);
    }
}
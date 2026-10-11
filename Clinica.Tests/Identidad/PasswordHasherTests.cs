using Clinica.Infraestructura.Identidad.Seguridad;

namespace Clinica.Tests.Identidad;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Verificar_ConLaMismaPassword_DevuelveTrue()
    {
        var hash = _sut.Hashear("Secreta123");

        Assert.True(_sut.Verificar("Secreta123", hash));
    }

    [Fact]
    public void Verificar_ConOtraPassword_DevuelveFalse()
    {
        var hash = _sut.Hashear("Secreta123");

        Assert.False(_sut.Verificar("otra-clave", hash));
    }

    [Fact]
    public void Hashear_DosVecesLaMismaPassword_DaHashesDistintos()
    {
        Assert.NotEqual(_sut.Hashear("Secreta123"), _sut.Hashear("Secreta123"));
    }

    [Fact]
    public void Verificar_ConHashInvalido_DevuelveFalse()
    {
        Assert.False(_sut.Verificar("Secreta123", "esto-no-es-un-hash"));
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Dominio.Comun;
using Clinica.Dominio.HistoriaClinica;

namespace Clinica.Tests.HistoriaClinica;

public class ArchivoAdjuntoTests
{
    private static ArchivoAdjunto CrearValido(
        int entradaId = 1,
        string nombre = "radiografia.jpg",
        string tipoContenido = "image/jpeg",
        long tamano = 1024,
        string clave = "abc123",
        int usuarioId = 2)
        => new(entradaId, nombre, tipoContenido, tamano, clave, usuarioId);

    [Fact]
    public void Constructor_CuandoLosDatosSonValidos_EntoncesGuardaLosMetadatos()
    {
        var adjunto = new ArchivoAdjunto(4, "  panoramica.jpg  ", " IMAGE/JPEG ", 2048, "clave-1", 9);

        Assert.Equal(4, adjunto.EntradaHistoriaClinicaId);
        Assert.Equal("panoramica.jpg", adjunto.NombreOriginal);
        Assert.Equal("image/jpeg", adjunto.TipoContenido);
        Assert.Equal(TipoAdjunto.Imagen, adjunto.Tipo);
        Assert.Equal(2048, adjunto.TamanoBytes);
        Assert.Equal("clave-1", adjunto.ClaveAlmacenamiento);
        Assert.Equal(9, adjunto.SubidoPorUsuarioId);
    }

    [Theory]
    [InlineData("rx.jpg", "image/jpeg", TipoAdjunto.Imagen)]
    [InlineData("rx.jpeg", "image/jpeg", TipoAdjunto.Imagen)]
    [InlineData("rx.png", "image/png", TipoAdjunto.Imagen)]
    [InlineData("estudio.pdf", "application/pdf", TipoAdjunto.Documento)]
    public void Constructor_CuandoElFormatoEstaPermitido_EntoncesDerivaElTipo(
        string nombre, string tipoContenido, TipoAdjunto esperado)
    {
        var adjunto = CrearValido(nombre: nombre, tipoContenido: tipoContenido);

        Assert.Equal(esperado, adjunto.Tipo);
    }

    [Fact]
    public void Constructor_CuandoLaExtensionEstaEnMayusculas_EntoncesLaAcepta()
    {
        var adjunto = CrearValido(nombre: "RX.JPG");

        Assert.Equal(TipoAdjunto.Imagen, adjunto.Tipo);
    }

    [Theory]
    [InlineData(@"C:\fotos\rx.png")]
    [InlineData("../../rx.png")]
    public void Constructor_CuandoElNombreTraeUnaRuta_EntoncesGuardaSoloElNombre(string nombre)
    {
        var adjunto = CrearValido(nombre: nombre, tipoContenido: "image/png");

        Assert.Equal("rx.png", adjunto.NombreOriginal);
    }

    [Theory]
    [InlineData("programa.exe")]
    [InlineData("nota.txt")]
    [InlineData("radiografia.gif")]
    [InlineData("sinextension")]
    public void Constructor_CuandoElFormatoNoEstaPermitido_EntoncesLanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => CrearValido(nombre: nombre, tipoContenido: "application/octet-stream"));
    }

    [Fact]
    public void Constructor_CuandoElTipoDeContenidoNoCoincideConLaExtension_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearValido(nombre: "rx.jpg", tipoContenido: "application/pdf"));
    }

    [Fact]
    public void Constructor_CuandoElTipoDeContenidoEstaEnBlanco_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearValido(tipoContenido: "   "));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_CuandoElArchivoEstaVacio_EntoncesLanzaDomainException(long tamano)
    {
        Assert.Throws<DomainException>(() => CrearValido(tamano: tamano));
    }

    [Fact]
    public void Constructor_CuandoElTamanoEsExactamenteElMaximo_EntoncesLoAcepta()
    {
        var adjunto = CrearValido(tamano: ArchivoAdjunto.TamanoMaximoBytes);

        Assert.Equal(ArchivoAdjunto.TamanoMaximoBytes, adjunto.TamanoBytes);
    }

    [Fact]
    public void Constructor_CuandoElTamanoSuperaElMaximo_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearValido(tamano: ArchivoAdjunto.TamanoMaximoBytes + 1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_CuandoElNombreEstaEnBlanco_EntoncesLanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => CrearValido(nombre: nombre));
    }

    [Fact]
    public void Constructor_CuandoElNombreSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var nombre = new string('a', ArchivoAdjunto.LongitudMaximaNombre - 3) + ".pdf";

        Assert.Throws<DomainException>(() => CrearValido(nombre: nombre, tipoContenido: "application/pdf"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_CuandoLaEntradaNoEsValida_EntoncesLanzaDomainException(int entradaId)
    {
        Assert.Throws<DomainException>(() => CrearValido(entradaId: entradaId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_CuandoElUsuarioNoEsValido_EntoncesLanzaDomainException(int usuarioId)
    {
        Assert.Throws<DomainException>(() => CrearValido(usuarioId: usuarioId));
    }

    [Fact]
    public void Constructor_CuandoLaClaveEstaEnBlanco_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CrearValido(clave: "  "));
    }

    [Fact]
    public void Constructor_CuandoLaClaveSuperaLaLongitudMaxima_EntoncesLanzaDomainException()
    {
        var clave = new string('k', ArchivoAdjunto.LongitudMaximaClave + 1);

        Assert.Throws<DomainException>(() => CrearValido(clave: clave));
    }

    [Fact]
    public void Validar_CuandoElArchivoEsValido_EntoncesDevuelveElTipo()
    {
        var tipo = ArchivoAdjunto.Validar("estudio.pdf", "application/pdf", 500);

        Assert.Equal(TipoAdjunto.Documento, tipo);
    }

    [Fact]
    public void Validar_CuandoElFormatoNoEstaPermitido_EntoncesLanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ArchivoAdjunto.Validar("virus.exe", "application/octet-stream", 500));
    }
}
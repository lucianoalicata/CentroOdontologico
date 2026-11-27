using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Infraestructura.HistoriaClinica.Almacenamiento;
using Microsoft.Extensions.Options;

namespace Clinica.Tests.HistoriaClinica;

public class AlmacenamientoLocalArchivosTests : IDisposable
{
    // Cada test usa su propia carpeta temporal (xUnit crea una instancia de la clase por test).
    private readonly string _carpeta =
        Path.Combine(Path.GetTempPath(), "clinica-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }

    private AlmacenamientoLocalArchivos CrearAlmacenamiento() =>
        new(Options.Create(new AlmacenamientoSettings { RutaBase = _carpeta }));

    private static MemoryStream Contenido(string texto) => new(Encoding.UTF8.GetBytes(texto));

    [Fact]
    public async Task GuardarAsync_CuandoSeGuardaUnArchivo_EntoncesSePuedeLeerConElMismoContenido()
    {
        var almacenamiento = CrearAlmacenamiento();

        var clave = await almacenamiento.GuardarAsync(Contenido("contenido de la radiografia"), "rx.jpg");

        await using var stream = await almacenamiento.AbrirLecturaAsync(clave);
        using var lector = new StreamReader(stream);
        Assert.Equal("contenido de la radiografia", await lector.ReadToEndAsync());
    }

    [Fact]
    public async Task GuardarAsync_CuandoLaCarpetaNoExiste_EntoncesLaCrea()
    {
        var almacenamiento = CrearAlmacenamiento();
        Assert.False(Directory.Exists(_carpeta));

        await almacenamiento.GuardarAsync(Contenido("datos"), "rx.png");

        Assert.True(Directory.Exists(_carpeta));
    }

    [Fact]
    public async Task GuardarAsync_CuandoElNombreTieneExtension_EntoncesLaClaveLaConservaEnMinusculas()
    {
        var almacenamiento = CrearAlmacenamiento();

        var clave = await almacenamiento.GuardarAsync(Contenido("datos"), "Panoramica.PNG");

        Assert.EndsWith(".png", clave);
    }

    [Theory]
    [InlineData("sinextension")]
    [InlineData("archivo.")]
    [InlineData("archivo.p;g")]
    [InlineData("archivo.extensionmuylarga")]
    public async Task GuardarAsync_CuandoLaExtensionNoEsSegura_EntoncesLaClaveQuedaSinExtension(string nombre)
    {
        var almacenamiento = CrearAlmacenamiento();

        var clave = await almacenamiento.GuardarAsync(Contenido("datos"), nombre);

        Assert.Equal(string.Empty, Path.GetExtension(clave));
    }

    [Fact]
    public async Task GuardarAsync_CuandoSeGuardaDosVecesElMismoNombre_EntoncesDevuelveClavesDistintas()
    {
        var almacenamiento = CrearAlmacenamiento();

        var primera = await almacenamiento.GuardarAsync(Contenido("uno"), "rx.jpg");
        var segunda = await almacenamiento.GuardarAsync(Contenido("dos"), "rx.jpg");

        Assert.NotEqual(primera, segunda);
    }

    [Fact]
    public async Task GuardarAsync_CuandoLaCopiaFalla_EntoncesNoDejaArchivosParciales()
    {
        var almacenamiento = CrearAlmacenamiento();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => almacenamiento.GuardarAsync(new FlujoQueFalla(), "rx.jpg"));

        Assert.Empty(Directory.GetFiles(_carpeta));
    }

    [Fact]
    public async Task AbrirLecturaAsync_CuandoLaClaveNoExiste_EntoncesLanzaNoEncontradoException()
    {
        var almacenamiento = CrearAlmacenamiento();

        await Assert.ThrowsAsync<NoEncontradoException>(() => almacenamiento.AbrirLecturaAsync("noexiste.jpg"));
    }

    [Theory]
    [InlineData("../secreto.txt")]
    [InlineData(@"..\secreto.txt")]
    [InlineData("subcarpeta/archivo.jpg")]
    [InlineData("..")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AbrirLecturaAsync_CuandoLaClaveNoEsUnNombreSimple_EntoncesLanzaNoEncontradoException(string clave)
    {
        var almacenamiento = CrearAlmacenamiento();

        await Assert.ThrowsAsync<NoEncontradoException>(() => almacenamiento.AbrirLecturaAsync(clave));
    }

    [Fact]
    public async Task AbrirLecturaAsync_CuandoLaClaveApuntaAUnArchivoFueraDeLaCarpeta_EntoncesLanzaNoEncontradoException()
    {
        var almacenamiento = CrearAlmacenamiento();
        var nombreAfuera = "secreto-" + Guid.NewGuid().ToString("N") + ".txt";
        var rutaAfuera = Path.Combine(Path.GetTempPath(), nombreAfuera);
        await File.WriteAllTextAsync(rutaAfuera, "no se tiene que poder leer");

        try
        {
            await Assert.ThrowsAsync<NoEncontradoException>(
                () => almacenamiento.AbrirLecturaAsync("../" + nombreAfuera));
        }
        finally
        {
            File.Delete(rutaAfuera);
        }
    }

    [Fact]
    public async Task EliminarAsync_CuandoElArchivoExiste_EntoncesYaNoSePuedeAbrir()
    {
        var almacenamiento = CrearAlmacenamiento();
        var clave = await almacenamiento.GuardarAsync(Contenido("datos"), "rx.jpg");

        await almacenamiento.EliminarAsync(clave);

        await Assert.ThrowsAsync<NoEncontradoException>(() => almacenamiento.AbrirLecturaAsync(clave));
    }

    [Fact]
    public async Task EliminarAsync_CuandoElArchivoYaNoExiste_EntoncesNoLanzaExcepcion()
    {
        var almacenamiento = CrearAlmacenamiento();

        await almacenamiento.EliminarAsync("noexiste.jpg");
    }

    [Fact]
    public async Task EliminarAsync_CuandoLaClaveApuntaAUnArchivoFueraDeLaCarpeta_EntoncesNoLoBorra()
    {
        var almacenamiento = CrearAlmacenamiento();
        var nombreAfuera = "secreto-" + Guid.NewGuid().ToString("N") + ".txt";
        var rutaAfuera = Path.Combine(Path.GetTempPath(), nombreAfuera);
        await File.WriteAllTextAsync(rutaAfuera, "no se tiene que borrar");

        try
        {
            await almacenamiento.EliminarAsync("../" + nombreAfuera);

            Assert.True(File.Exists(rutaAfuera));
        }
        finally
        {
            File.Delete(rutaAfuera);
        }
    }

    // Flujo que falla al leer, para simular una copia interrumpida a mitad de camino.
    private sealed class FlujoQueFalla : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new InvalidOperationException("Fallo simulado.");

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Fallo simulado.");

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
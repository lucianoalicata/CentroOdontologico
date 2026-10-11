using Clinica.Aplicacion.Identidad.Dtos;

namespace Clinica.Aplicacion.Identidad.Puertos;

public interface IJwtProvider
{
    // Recibe la sesión (no la entidad): así el proveedor no conoce el dominio.
    TokenGenerado Generar(UsuarioSesionDto sesion);
}

public record TokenGenerado(string Token, DateTime ExpiraEn);
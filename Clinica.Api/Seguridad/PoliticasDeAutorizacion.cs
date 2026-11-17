using Clinica.Dominio.Identidad;
using Microsoft.AspNetCore.Authorization;

namespace Clinica.Api.Seguridad;

public static class PoliticasDeAutorizacion
{
    public static void Configurar(AuthorizationOptions opciones)
    {
        opciones.AddPolicy(Politicas.SoloPersonal, politica =>
            politica.RequireAuthenticatedUser().RequireRole(
                nameof(RolUsuario.Secretaria),
                nameof(RolUsuario.Odontologo),
                nameof(RolUsuario.Administrador)));

        opciones.AddPolicy(Politicas.SoloOdontologo, politica =>
            politica.RequireAuthenticatedUser().RequireRole(nameof(RolUsuario.Odontologo)));

        opciones.AddPolicy(Politicas.SoloAdministrador, politica =>
            politica.RequireAuthenticatedUser().RequireRole(nameof(RolUsuario.Administrador)));
    }
}
using Microsoft.AspNetCore.Authorization;

namespace Clinica.Api.Seguridad
{
    public class PoliticasDeAutorizacion
    {
        public static void Configurar(AuthorizationOptions opciones)
        {
            opciones.AddPolicy(Politicas.SoloPersonal.ToString(), politica =>
            {
                politica.RequireClaim("Rol", "Personal");
            });
            opciones.AddPolicy(Politicas.SoloAdministrador.ToString(), politica =>
            {
                politica.RequireClaim("Rol", "Administrador");
            });
            opciones.AddPolicy(Politicas.SoloOdontologo.ToString(), politica =>
            {
                politica.RequireClaim("Rol", "Odontologo");
            });
        }
    }
}

namespace Clinica.Api.Seguridad
{
    public static class Politicas
    {
        public const string SoloPersonal = nameof(SoloPersonal);
        public const string SoloOdontologo = nameof(SoloOdontologo);
        public const string SoloAdministrador = nameof(SoloAdministrador);
    }
}

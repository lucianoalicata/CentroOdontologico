namespace Clinica.Aplicacion.Identidad.Puertos;

public interface IPasswordHasher
{
    string Hashear(string password);
    bool Verificar(string password, string hash);
}
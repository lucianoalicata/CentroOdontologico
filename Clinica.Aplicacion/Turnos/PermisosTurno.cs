using Clinica.Aplicacion.Identidad;
using Clinica.Dominio.Comun;
using Clinica.Dominio.Identidad;
using Clinica.Dominio.Turnos;

namespace Clinica.Aplicacion.Turnos;

/// <summary>Reglas de rol compartidas por los casos de uso de Turnos.</summary>
public static class PermisosTurno
{
    /// <summary>Secretaría o administrador: gestionan la agenda de cualquier odontólogo.</summary>
    public static bool EsPersonal(IUsuarioActual usuario) =>
        usuario.TieneRol(nameof(RolUsuario.Secretaria)) || usuario.TieneRol(nameof(RolUsuario.Administrador));

    public static bool EsOdontologo(IUsuarioActual usuario) =>
        usuario.TieneRol(nameof(RolUsuario.Odontologo));

    public static bool EsPaciente(IUsuarioActual usuario) =>
        usuario.TieneRol(nameof(RolUsuario.Paciente));

    /// <summary>
    /// Origen elegido por el personal: telefónico o presencial (presencial si no se indica).
    /// </summary>
    public static OrigenCreacion OrigenDelPersonal(OrigenCreacion? pedido)
    {
        var origen = pedido ?? OrigenCreacion.PresencialSecretaria;
        if (origen is not (OrigenCreacion.TelefonoSecretaria or OrigenCreacion.PresencialSecretaria))
            throw new DomainException("El origen del turno debe ser telefónico o presencial.");
        return origen;
    }
}

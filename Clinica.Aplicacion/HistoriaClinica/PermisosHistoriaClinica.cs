using System;
using System.Collections.Generic;
using System.Text;
using Clinica.Aplicacion.Comun.Excepciones;
using Clinica.Aplicacion.Identidad;
using Clinica.Dominio.Identidad;

namespace Clinica.Aplicacion.HistoriaClinica;

/// <summary>Reglas de rol compartidas por los casos de uso de la historia clínica.</summary>
public static class PermisosHistoriaClinica
{
    /// <summary>Ver la historia: odontólogo o administrador.</summary>
    public static void ExigirConsulta(IUsuarioActual usuario)
    {
        if (!usuario.TieneRol(nameof(RolUsuario.Odontologo)) && !usuario.TieneRol(nameof(RolUsuario.Administrador)))
            throw new AccesoDenegadoException("No tenés permisos para ver la historia clínica.");
    }

    /// <summary>Modificar la historia: solo un odontólogo. Devuelve su OdontologoId.</summary>
    public static int ExigirOdontologo(IUsuarioActual usuario)
    {
        if (!usuario.TieneRol(nameof(RolUsuario.Odontologo)) || usuario.OdontologoId is not int odontologoId)
            throw new AccesoDenegadoException("Solo un odontólogo puede modificar la historia clínica.");
        return odontologoId;
    }
}
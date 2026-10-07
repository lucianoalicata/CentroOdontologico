using System;
using System.Collections.Generic;
using System.Text;

namespace Clinica.Dominio.Pagos
{
    public enum EstadoTransaccion
    {
        Creada, Pendiente, Aprobada,
        Rechazada, Cancelada, Expirada
    }
}

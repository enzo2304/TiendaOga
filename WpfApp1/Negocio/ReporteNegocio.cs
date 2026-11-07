using System;
using System.Collections.Generic;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class ReporteNegocio
    {
        private static readonly ReporteDatos _datos = new ReporteDatos();

        public static bool TienePermisoReportes(string rol, out string mensajeError)
        {
            if (string.Equals(rol, "Vendedor", StringComparison.OrdinalIgnoreCase))
            {
                mensajeError = "Acceso denegado: El perfil Vendedor no tiene autorización para visualizar métricas globales y reportes de la tienda.";
                return false;
            }
            mensajeError = string.Empty;
            return true;
        }

        // Ahora recibe un rango de fechas (un mes completo o un día puntual)
        public static ReporteGeneralDTO ObtenerReporteGeneral(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
                throw new ArgumentException("La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.");

            return _datos.ObtenerReporteGeneral(desde.Date, hasta.Date);
        }
    }
}
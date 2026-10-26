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

        public static ReporteGeneralDTO ObtenerReporteGeneral(int mes, int anio)
        {
            return _datos.ObtenerReporteGeneral(mes, anio);
        }
    }
}
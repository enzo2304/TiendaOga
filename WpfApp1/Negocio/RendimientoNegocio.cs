using System;
using System.Collections.Generic;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class RendimientoNegocio
    {
        private static readonly RendimientoDatos _datos = new RendimientoDatos();

        public static RendimientoVendedorDTO ConsultarRendimiento(int idUsuario, DateTime desde, DateTime hasta)
        {
            return _datos.ObtenerRendimiento(idUsuario, desde, hasta);
        }

        public static List<dynamic> ObtenerVendedoresActivos()
        {
            return _datos.ObtenerVendedoresActivos();
        }
    }
}
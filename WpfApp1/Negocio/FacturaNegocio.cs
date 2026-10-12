using System;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class FacturaNegocio
    {
        public static Factura Obtener(int idVenta)
        {
            var f = new FacturaDatos().Obtener(idVenta);
            if (f == null)
                throw new InvalidOperationException("No se encontró la venta para generar el comprobante.");
            return f;
        }

        public static void GuardarPdf(Factura f, string ruta)
        {
            FacturaPdf.Generar(f, ruta);
        }
    }
}
using System;
using System.IO;
using TiendaOga.Datos;

namespace TiendaOga.Negocio
{
    public static class FacturaNegocio
    {
        public static string GenerarPdf(int idVenta)
        {
            var factura = new FacturaDatos().Obtener(idVenta);
            if (factura == null)
                throw new InvalidOperationException("No se encontró la venta para generar la factura.");

            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "TiendaOga", "Facturas");
            Directory.CreateDirectory(carpeta);

            string ruta = Path.Combine(carpeta, "Factura_" + factura.NumeroVenta + ".pdf");
            FacturaPdf.Generar(factura, ruta);
            return ruta;
        }
    }
}
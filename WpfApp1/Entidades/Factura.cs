using System;
using System.Collections.Generic;

namespace TiendaOga.Entidades
{
    public class Factura
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal TotalVenta { get; set; }
        public string Vendedor { get; set; }
        public string NombreCliente { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public List<FacturaItem> Items { get; set; } = new List<FacturaItem>();
        public List<FacturaPago> Pagos { get; set; } = new List<FacturaPago>();
        public string Estado { get; set; }
    }

    public class FacturaItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class FacturaPago
    {
        public string TipoPago { get; set; }
        public decimal Monto { get; set; }
    }
}

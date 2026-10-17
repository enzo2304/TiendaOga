using System;
using System.Collections.Generic;

namespace TiendaOga.Entidades
{
    public class RendimientoVendedorDTO
    {
        // 1. KPIs Generales
        public int CantidadVentas { get; set; }
        public decimal VolumenFacturacion { get; set; }
        public decimal TicketPromedio { get; set; }
        public int CantidadAnuladas { get; set; }
        public double TasaAnulacion { get; set; }

        // 2. Listas para los gráficos y tablas
        public List<PagoRendimiento> MetodosDePago { get; set; } = new List<PagoRendimiento>();
        public List<ProductoRendimiento> TopProductos { get; set; } = new List<ProductoRendimiento>();
    }

    public class PagoRendimiento
    {
        public string Metodo { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductoRendimiento
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
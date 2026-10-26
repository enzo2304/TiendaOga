using System.Collections.Generic;

namespace TiendaOga.Entidades
{
    public class ReporteGeneralDTO

    {

        public List<ReporteProductoRow> Productos { get; set; } = new List<ReporteProductoRow>();

        public List<PagoRendimiento> MetodosDePagoGenerales { get; set; } = new List<PagoRendimiento>();

        public List<PagoPorProducto> PagosPorProducto { get; set; } = new List<PagoPorProducto>();

    }
}
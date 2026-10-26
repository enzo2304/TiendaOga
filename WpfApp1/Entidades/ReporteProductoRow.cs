using System;

namespace TiendaOga.Entidades
{
    public class ReporteProductoRow

    {

        public int Posicion { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalIngreso { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal GananciaNeta { get; set; }
        public int StockActual { get; set; }

    }
}
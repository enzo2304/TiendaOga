using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TiendaOga.Entidades
{
    public class VentaRegistrada
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; }
        public decimal Total { get; set; }
    }

    public class TipoPagoItem
    {
        public int IdTipoPago { get; set; }
        public string Nombre { get; set; }
    }
}
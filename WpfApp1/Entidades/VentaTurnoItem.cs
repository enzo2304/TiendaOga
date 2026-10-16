using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOga.Entidades
{
    public class VentaTurnoItem
    {
        public string NroComprobante { get; set; }
        public string Hora { get; set; }
        public string NombreCliente { get; set; }
        public string Detalle { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
    }
}

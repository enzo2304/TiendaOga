using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    /// Este DTO mapea el resultado del stored procedure sp_cliente_historial, que concatena los artículos y medios de pago de cada venta

namespace TiendaOga.Entidades
    {
        // Fila que devuelve sp_cliente_historial
        public class CompraRow
        {
            public string NumeroVenta { get; set; }
            public DateTime FechaVenta { get; set; }
            public string Articulos { get; set; }
            public string MetodoPago { get; set; }
            public decimal TotalVenta { get; set; }
        }
    }
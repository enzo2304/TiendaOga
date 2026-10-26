using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiendaOga.Entidades
{
    public class PagoPorProducto

    {
        public int IdProducto { get; set; }
        public string Metodo { get; set; }
        public decimal Total { get; set; }

    }
}

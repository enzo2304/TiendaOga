using System;

namespace TiendaOga.Entidades
{
    // La nota de crédito reutiliza los datos de la venta original (Factura)
    public class NotaCredito
    {
        public string NumeroNC { get; set; }
        public Factura Venta { get; set; }
        public DateTime FechaAnulacion { get; set; }
        public string Anulador { get; set; }
    }
}
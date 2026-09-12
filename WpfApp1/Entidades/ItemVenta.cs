namespace TiendaOga.Entidades
{
    /// <summary>
    /// Ítem de detalle de venta.
    /// </summary>
    public class ItemVenta
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => PrecioVenta * Cantidad;
    }
}

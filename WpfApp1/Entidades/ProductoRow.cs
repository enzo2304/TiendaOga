namespace TiendaOga.Entidades
{
    public class ProductoRow
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string NombreCategoria { get; set; }
        public decimal PrecioVentas { get; set; }
        public int Stock { get; set; }
        public string TipoProducto { get; set; } // "Hogar" o "Tecnologia"
    }
}

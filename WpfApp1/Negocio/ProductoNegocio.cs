namespace TiendaOga.Negocio
{
    /// <summary>
    /// Reglas de validación del formulario de Productos.
    /// No conoce controles de UI: recibe texto plano y devuelve
    /// si es válido más un mensaje de error listo para mostrar.
    /// </summary>
    public static class ProductoNegocio
    {
        public static bool ValidarProducto(string nombre, string precioVentaTexto, string stockTexto, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre del producto es obligatorio.";
                return false;
            }

            if (!decimal.TryParse(precioVentaTexto, out _))
            {
                mensajeError = "El precio de venta debe ser un número válido.";
                return false;
            }

            if (!int.TryParse(stockTexto, out _))
            {
                mensajeError = "El stock debe ser un número entero válido.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }
    }
}

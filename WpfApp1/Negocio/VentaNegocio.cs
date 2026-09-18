using System.Collections.Generic;
using System.Linq;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    /// <summary>
    /// Reglas de cálculo y validación de una venta:
    /// stock, totales, vuelto y precondiciones de guardado.
    /// </summary>
    public static class VentaNegocio
    {
        public static bool ValidarStockDisponible(int stockTotal, int cantidadEnGrilla, int cantidadNueva, out string mensajeError)
        {
            if (cantidadNueva <= 0)
            {
                mensajeError = "La cantidad debe ser un número entero mayor a cero.";
                return false;
            }

            if ((cantidadEnGrilla + cantidadNueva) > stockTotal)
            {
                mensajeError = $"Stock insuficiente. Disponible: {stockTotal} unidad(es). Ya agregadas en la orden: {cantidadEnGrilla}.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static decimal CalcularTotal(IEnumerable<ItemVenta> detalleVenta)
        {
            return detalleVenta?.Sum(item => item.Subtotal) ?? 0m;
        }

        public static decimal CalcularVuelto(decimal total, decimal montoRecibido)
        {
            return montoRecibido - total;
        }

        public static bool ValidarCantidad(int cantidad, out string mensajeError)
        {
            if (cantidad <= 0)
            {
                mensajeError = "La cantidad debe ser un número entero mayor a cero.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static bool ValidarVenta(int cantidadItems, decimal total, decimal montoRecibido, out string mensajeError)
        {
            if (cantidadItems == 0)
            {
                mensajeError = "Agregá al menos un producto antes de guardar la venta.";
                return false;
            }

            if (total <= 0)
            {
                mensajeError = "El monto total de la venta debe ser mayor a 0.";
                return false;
            }

            if (montoRecibido < total)
            {
                mensajeError = "El monto recibido es menor al total de la venta.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }
    }
}
using System.Collections.Generic;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    /// <summary>
    /// Reglas de cálculo y validación de una venta:
    /// total del detalle, vuelto, y condiciones para poder guardar.
    /// </summary>
    public static class VentaNegocio
    {
        public static decimal CalcularTotal(IEnumerable<ItemVenta> detalleVenta)
        {
            decimal total = 0;
            foreach (var item in detalleVenta)
            {
                total += item.Subtotal;
            }
            return total;
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

using System;
using System.Collections.Generic;
using System.Linq;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    /// <summary>
    /// Reglas de cálculo, validación y persistencia de una venta:
    /// stock, totales, vuelto y registro de transacciones.
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

        public static bool ValidarClienteHabilitado(ClienteItem cliente, out string mensajeError)
        {
            if (cliente == null)
            {
                mensajeError = "Debes seleccionar un cliente registrado para efectuar la venta.";
                return false;
            }

            if (!cliente.Activo)
            {
                mensajeError = $"El cliente \"{cliente.NombreCompleto}\" se encuentra DADO DE BAJA y no puede realizar compras.\nDebe reactivarse previamente en el padrón de Clientes.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static bool ValidarFechaVenta(DateTime fechaPago, out string mensajeError)
        {
            if (fechaPago.Date != DateTime.Today)
            {
                mensajeError = "La venta solo puede registrarse con la fecha actual del turno en curso.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Transacción completa de negocio: descuenta inventario y asienta la compra en el historial.
        /// </summary>
        public static void RegistrarVenta(int idCliente, IEnumerable<ItemVenta> detalle, decimal total, string metodoPago, DateTime fecha)
        {
            if (detalle == null) return;

            var items = detalle.ToList();

            // 1. Regla de Inventario: descontar stock
            foreach (var item in items)
            {
                var prod = DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == item.IdProducto);
                if (prod != null)
                {
                    prod.Stock -= item.Cantidad;
                }
            }

            // 2. Registro histórico
            var listaNombres = items.Select(item => $"{item.Nombre} x{item.Cantidad}").ToList();
            string resumenArticulos = string.Join(", ", listaNombres);

            DatosGlobales.RegistrarCompraCliente(
                idCliente: idCliente,
                detalle: resumenArticulos,
                total: total,
                metodoPago: metodoPago,
                fecha: fecha
            );
        }
    }
}
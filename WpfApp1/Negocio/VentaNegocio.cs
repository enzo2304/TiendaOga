using System;
using System.Collections.Generic;
using System.Linq;
using TiendaOga.Entidades;
using TiendaOga.Datos;

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
        public static VentaRegistrada RegistrarVenta(int idUsuario, int idCliente, int idTipoPago,
                                              IEnumerable<ItemVenta> detalle)
        {
            return new VentaDatos().Registrar(idUsuario, idCliente, idTipoPago, detalle);
        }

        /// <summary>
        /// Anula una venta: marca el estado, registra quién la anuló y devuelve el stock.
        /// </summary>
        public static void AnularVenta(int idVenta, int idUsuarioAnulo)
        {
            if (idVenta <= 0) throw new ArgumentException("Venta inválida.");
            if (idUsuarioAnulo <= 0) throw new ArgumentException("Usuario inválido.");

            new VentaDatos().AnularVenta(idVenta, idUsuarioAnulo);
        }
    }
}
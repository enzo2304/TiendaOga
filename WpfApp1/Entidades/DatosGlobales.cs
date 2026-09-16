using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TiendaOga.Entidades
{
    public class CompraCliente
    {
        public string NroComprobante { get; set; }
        public string Fecha { get; set; }
        public string DetalleProductos { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
    }

    public class ClienteItem
    {
        public int IdCliente { get; set; }
        public string Dni { get; set; }
        public string NombreCompleto { get; set; }
        public string Telefono { get; set; }
        public string TipoCliente { get; set; } = "Consumidor Final";
        public string TipoDocumento { get; set; } = "DNI";
        public bool Activo { get; set; } = true;
        public int CantidadCompras => HistorialCompras.Count;
        public List<CompraCliente> HistorialCompras { get; set; } = new List<CompraCliente>();
    }


    public static class DatosGlobales
    {
        public static ObservableCollection<ClienteItem> Clientes { get; set; } = new ObservableCollection<ClienteItem>();

        public static List<ProductoRow> Productos { get; set; } = new List<ProductoRow>
    {
        new ProductoRow { IdProducto = 1, NombreProducto = "Sofá 3 Cuerpos", NombreCategoria = "Hogar", PrecioVentas = 185000, Stock = 5, TipoProducto = "Hogar" },
        new ProductoRow { IdProducto = 2, NombreProducto = "Sofá Cama", NombreCategoria = "Hogar", PrecioVentas = 145000, Stock = 3, TipoProducto = "Hogar" },
        new ProductoRow { IdProducto = 3, NombreProducto = "Foco Inteligente LED Wi-Fi", NombreCategoria = "Tecnologia", PrecioVentas = 3200, Stock = 40, TipoProducto = "Tecnologia" },
        new ProductoRow { IdProducto = 4, NombreProducto = "Cámara de Seguridad Interior", NombreCategoria = "Tecnologia", PrecioVentas = 18900, Stock = 12, TipoProducto = "Tecnologia" },
        new ProductoRow { IdProducto = 5, NombreProducto = "Altavoz Asistente de Voz", NombreCategoria = "Tecnologia", PrecioVentas = 21500, Stock = 20, TipoProducto = "Tecnologia" },
        new ProductoRow { IdProducto = 6, NombreProducto = "Campera Rompeviento", NombreCategoria = "Accesorios", PrecioVentas = 32000, Stock = 15, TipoProducto = "Accesorios" }
    };

        public static void RegistrarCompraCliente(string nombre, string dni, string telefono, string detalle, decimal total, string metodoPago, DateTime? fecha = null)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string dniLimpio = string.IsNullOrWhiteSpace(dni) ? "S/D" : dni.Trim();
            string nombreLimpio = nombre.Trim();

            var cliente = Clientes.FirstOrDefault(c =>
                (!string.IsNullOrEmpty(dni) && c.Dni == dniLimpio) ||
                c.NombreCompleto.Equals(nombreLimpio, StringComparison.OrdinalIgnoreCase));

            DateTime fechaPago = fecha ?? DateTime.Now;

            var nuevaCompra = new CompraCliente
            {
                NroComprobante = $"VTA-{DateTime.Now:HHmmss}",
                Fecha = fechaPago.ToString("dd/MM/yyyy HH:mm"),
                DetalleProductos = detalle,
                Total = total,
                MetodoPago = string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago
            };

            if (cliente == null)
            {
                cliente = new ClienteItem
                {
                    IdCliente = Clientes.Count + 1,
                    Dni = dniLimpio,
                    NombreCompleto = nombreLimpio,
                    Telefono = string.IsNullOrWhiteSpace(telefono) ? "S/D" : telefono.Trim()
                };
                cliente.HistorialCompras.Insert(0, nuevaCompra);
                Clientes.Insert(0, cliente);
            }
            else
            {
                cliente.HistorialCompras.Insert(0, nuevaCompra);
            }
        }
    }
}
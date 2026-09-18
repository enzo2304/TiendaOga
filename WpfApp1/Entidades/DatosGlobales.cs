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

        // Propiedad de lectura formateada para el ComboBox
        public string NombreDisplay => string.IsNullOrWhiteSpace(Dni) || Dni == "S/D"
            ? NombreCompleto
            : $"{NombreCompleto} (DNI: {Dni})";

        // Esto garantiza que si el ComboBox no usa template, dibuje el texto igual
        public override string ToString()
        {
            return NombreDisplay;
        }
    }

    public static class DatosGlobales
    {
        public static ObservableCollection<ClienteItem> Clientes { get; set; } = new ObservableCollection<ClienteItem>
        {
            new ClienteItem
            {
                IdCliente = 1,
                NombreCompleto = "Consumidor Final",
                Dni = "S/D",
                Telefono = "00000000",
                TipoCliente = "Consumidor Final"
            }
        };

        public static List<ProductoRow> Productos { get; set; } = new List<ProductoRow>
        {
            new ProductoRow { IdProducto = 1, NombreProducto = "Taladro Percutor 650W", NombreCategoria = "Herramientas", PrecioCosto = 15000, PrecioVentas = 25000, Stock = 10, TipoProducto = "Tecnología" },
            new ProductoRow { IdProducto = 2, NombreProducto = "Juego de Ollas 5 piezas", NombreCategoria = "Hogar", PrecioCosto = 8000, PrecioVentas = 14500, Stock = 5, TipoProducto = "Hogar" },
            new ProductoRow { IdProducto = 3, NombreProducto = "Escoba de cerdas duras", NombreCategoria = "Limpieza", PrecioCosto = 1200, PrecioVentas = 2000, Stock = 30, TipoProducto = "Hogar" },
            new ProductoRow { IdProducto = 4, NombreProducto = "Foco Inteligente LED Wi-Fi", NombreCategoria = "Tecnología", PrecioCosto = 1800, PrecioVentas = 3200, Stock = 40, TipoProducto = "Tecnología" },
            new ProductoRow { IdProducto = 5, NombreProducto = "Cámara de Seguridad Interior", NombreCategoria = "Tecnología", PrecioCosto = 11000, PrecioVentas = 18900, Stock = 12, TipoProducto = "Tecnología" }
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
                    IdCliente = Clientes.Count == 0 ? 1 : Clientes.Max(c => c.IdCliente) + 1,
                    Dni = dniLimpio,
                    NombreCompleto = nombreLimpio,
                    Telefono = string.IsNullOrWhiteSpace(telefono) ? "00000000" : telefono.Trim()
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
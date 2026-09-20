using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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

    public class ClienteItem : INotifyPropertyChanged
    {
        private string _dni;
        private string _nombreCompleto;
        private string _telefono;
        private string _tipoCliente = "Cliente Registrado";
        private string _tipoDocumento = "DNI";
        private bool _activo = true;

        public int IdCliente { get; set; }

        public string Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); OnPropertyChanged(nameof(NombreDisplay)); }
        }

        public string NombreCompleto
        {
            get => _nombreCompleto;
            set { _nombreCompleto = value; OnPropertyChanged(); OnPropertyChanged(nameof(NombreDisplay)); }
        }

        public string Telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(); }
        }

        public string TipoCliente
        {
            get => _tipoCliente;
            set { _tipoCliente = value; OnPropertyChanged(); }
        }

        public string TipoDocumento
        {
            get => _tipoDocumento;
            set { _tipoDocumento = value; OnPropertyChanged(); }
        }

        // Baja lógica: al cambiar esto, la grilla se actualiza sola (fila roja / botón Reactivar)
        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        public int CantidadCompras => HistorialCompras.Count;
        public List<CompraCliente> HistorialCompras { get; set; } = new List<CompraCliente>();

        public string NombreDisplay => string.IsNullOrWhiteSpace(Dni) || Dni == "S/D"
            ? NombreCompleto
            : $"{NombreCompleto} (DNI: {Dni})";

        public override string ToString()
        {
            return NombreDisplay;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class DatosGlobales
    {
        public static ObservableCollection<ClienteItem> Clientes { get; set; } = new ObservableCollection<ClienteItem>
        {
            new ClienteItem
            {
                IdCliente = 1,
                NombreCompleto = "Enzo Sánchez",
                Dni = "30111222",
                Telefono = "3794123456",
                TipoCliente = "Consumidor Final",
                TipoDocumento = "DNI",
                Activo = true,
                HistorialCompras = new List<CompraCliente>
                {
                    new CompraCliente { NroComprobante = "VTA-000123", Fecha = "12/09/2026 10:15", DetalleProductos = "Taladro Percutor 650W x1", Total = 25000, MetodoPago = "Efectivo" },
                    new CompraCliente { NroComprobante = "VTA-000098", Fecha = "02/08/2026 17:40", DetalleProductos = "Foco Inteligente LED Wi-Fi x2", Total = 6400, MetodoPago = "Tarjeta" }
                }
            },
            new ClienteItem
            {
                IdCliente = 2,
                NombreCompleto = "María Gómez",
                Dni = "28555444",
                Telefono = "3794998877",
                TipoCliente = "Consumidor Final",
                TipoDocumento = "DNI",
                Activo = true,
                HistorialCompras = new List<CompraCliente>
                {
                    new CompraCliente { NroComprobante = "VTA-000110", Fecha = "05/09/2026 09:00", DetalleProductos = "Juego de Ollas 5 piezas x1", Total = 14500, MetodoPago = "Transferencia" }
                }
            },
            new ClienteItem
            {
                IdCliente = 3,
                NombreCompleto = "Carlos Ferretería del Norte SRL",
                Dni = "30887766993",
                Telefono = "3794555222",
                TipoCliente = "Responsable Inscripto",
                TipoDocumento = "CUIT",
                Activo = true,
                HistorialCompras = new List<CompraCliente>
                {
                    new CompraCliente { NroComprobante = "VTA-000075", Fecha = "20/07/2026 14:20", DetalleProductos = "Escoba de cerdas duras x10, Cámara de Seguridad Interior x1", Total = 38900, MetodoPago = "Efectivo" }
                }
            },
            new ClienteItem
            {
                IdCliente = 4,
                NombreCompleto = "Lucía Pérez",
                Dni = "35222111",
                Telefono = "3794333444",
                TipoCliente = "Consumidor Final",
                TipoDocumento = "DNI",
                Activo = false, // Cliente dado de baja, para ver la fila en rojo
                HistorialCompras = new List<CompraCliente>()
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

        public static void RegistrarCompraCliente(int idCliente, string detalle, decimal total, string metodoPago, DateTime? fecha = null)
        {
            var cliente = Clientes.FirstOrDefault(c => c.IdCliente == idCliente);
            if (cliente == null) return;

            DateTime fechaPago = fecha ?? DateTime.Now;

            var nuevaCompra = new CompraCliente
            {
                NroComprobante = $"VTA-{DateTime.Now:HHmmss}",
                Fecha = fechaPago.ToString("dd/MM/yyyy HH:mm"),
                DetalleProductos = detalle,
                Total = total,
                MetodoPago = string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago
            };

            cliente.HistorialCompras.Insert(0, nuevaCompra);
        }
    }
}
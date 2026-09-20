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
        public static ObservableCollection<ClienteItem> Clientes { get; set; } = new ObservableCollection<ClienteItem>();

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
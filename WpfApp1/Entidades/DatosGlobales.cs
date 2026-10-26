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

        public string Estado { get; set; }
        public int IdVenta { get; set; }
    }

    public class ClienteItem : INotifyPropertyChanged
    {
        private string _dni;
        private string _nombreCompleto;
        private string _telefono;
        private string _tipoCliente = "PERSONA";   // la base solo acepta PERSONA / EMPRESA
        private string _tipoDocumento = "DNI";
        private bool _activo = true;
        private int _cantidadCompras;

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

        // Antes era HistorialCompras.Count. Ahora la cantidad viene de la base (sp_cliente_listar_con_compras)
        public int CantidadCompras
        {
            get => _cantidadCompras;
            set { _cantidadCompras = value; OnPropertyChanged(); }
        }

        // Ya no se usa para mostrar el historial (se lee de la base con ClienteNegocio.ObtenerHistorial).
        // Se deja para no romper codigo que todavia lo referencie.
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
        // Arranca vacia: ClienteNegocio.ObtenerTodos() la llena desde la base de datos
        public static ObservableCollection<ClienteItem> Clientes { get; set; } = new ObservableCollection<ClienteItem>();

        public static List<ProductoRow> Productos { get; set; } = new List<ProductoRow>();

        // Mientras las ventas no se guarden en la base, esto sigue registrando la compra en memoria.
        // Cuando VentaNegocio grabe en venta_cabecera / venta_detalle / pago, este metodo deja de hacer falta.
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
            cliente.CantidadCompras++;
        }
    }
}
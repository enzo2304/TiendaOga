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

        public static void RegistrarCompraCliente(string nombre, string dni, string telefono, string detalle, decimal total, string metodoPago)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string dniLimpio = string.IsNullOrWhiteSpace(dni) ? "S/D" : dni.Trim();
            string nombreLimpio = nombre.Trim();

            var cliente = Clientes.FirstOrDefault(c =>
                (!string.IsNullOrEmpty(dni) && c.Dni == dniLimpio) ||
                c.NombreCompleto.Equals(nombreLimpio, StringComparison.OrdinalIgnoreCase));

            var nuevaCompra = new CompraCliente
            {
                NroComprobante = $"VTA-{DateTime.Now:HHmmss}",
                Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
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
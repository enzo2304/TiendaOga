using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class NotaCreditoWindow : Window
    {
        private const int FilasMinimas = 12;   // para que la hoja se vea como un formulario
        private readonly NotaCredito _nc;

        // Si falla (venta inexistente o no anulada) lanza excepción: quien abre la ventana la captura
        public NotaCreditoWindow(int idVenta)
        {
            InitializeComponent();
            _nc = NotaCreditoNegocio.Obtener(idVenta);
            CargarDatos();
        }

        private void CargarDatos()
        {
            var f = _nc.Venta;

            txtComercio.Text = DatosEmisor.Nombre;
            txtDireccion.Text = DatosEmisor.LineaDireccion;
            txtTelefono.Text = DatosEmisor.LineaTelefono;
            txtEmail.Text = DatosEmisor.Email;
            txtNumeroNC.Text = _nc.NumeroNC;

            txtDia.Text = _nc.FechaAnulacion.ToString("dd");
            txtMes.Text = _nc.FechaAnulacion.ToString("MM");
            txtAnio.Text = _nc.FechaAnulacion.ToString("yyyy");

            txtCliente.Text = f.NombreCliente;
            txtDocumento.Text = (f.TipoDocumento ?? "Doc.") + ": " + (f.NroDocumento ?? "-");
            txtNumeroVenta.Text = f.NumeroVenta;
            txtFechaVenta.Text = f.FechaVenta.ToString("dd/MM/yyyy");
            txtVendedor.Text = f.Vendedor;
            txtAnulador.Text = _nc.Anulador;
            txtPago.Text = string.Join(", ", f.Pagos.Select(p => p.TipoPago));

            // LineaTicket está definida en ComprobanteVentaWindow.xaml.cs (mismo namespace)
            var lineas = f.Items.Select(i => new LineaTicket
            {
                Cantidad = i.Cantidad.ToString(),
                Producto = i.Nombre,
                Precio = FormatoTicket.Moneda(i.PrecioUnitario),
                Subtotal = FormatoTicket.Moneda(i.SubTotal)
            }).ToList();

            while (lineas.Count < FilasMinimas)
                lineas.Add(new LineaTicket { Cantidad = "", Producto = "", Precio = "", Subtotal = "" });

            lstItems.ItemsSource = lineas;

            txtSubtotal.Text = FormatoTicket.Moneda(f.TotalVenta);
            txtTotal.Text = FormatoTicket.Moneda(f.TotalVenta);
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new PrintDialog();
            if (dialogo.ShowDialog() == true)
                dialogo.PrintVisual(Ticket, "Nota de crédito " + _nc.NumeroNC);
        }

        private void BtnGuardarPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "NotaCredito_" + _nc.NumeroNC,
                DefaultExt = ".pdf",
                Filter = "Archivo PDF (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog(this) != true) return;

            try
            {
                NotaCreditoNegocio.GuardarPdf(_nc, dlg.FileName);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar el PDF:\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
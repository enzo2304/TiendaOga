using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public class LineaTicket
    {
        public string Cantidad { get; set; }
        public string Producto { get; set; }
        public string Precio { get; set; }
        public string Subtotal { get; set; }
    }

    public partial class ComprobanteVentaWindow : Window
    {
        private readonly Factura _factura;

        public ComprobanteVentaWindow(int idVenta)
        {
            InitializeComponent();
            _factura = FacturaNegocio.Obtener(idVenta);
            CargarDatos();
        }

        private void CargarDatos()
        {
            txtComercio.Text = DatosEmisor.Nombre;
            txtSubtitulo.Text = DatosEmisor.Subtitulo;
            txtPie.Text = DatosEmisor.Pie;

            txtNumero.Text = _factura.NumeroVenta;
            txtFecha.Text = _factura.FechaVenta.ToString("dd/MM/yyyy HH:mm");
            txtCliente.Text = _factura.NombreCliente;
            txtVendedor.Text = _factura.Vendedor;
            txtPago.Text = string.Join(", ", _factura.Pagos.Select(p => p.TipoPago));
            txtTotal.Text = FormatoTicket.Moneda(_factura.TotalVenta);

            lstItems.ItemsSource = _factura.Items.Select(i => new LineaTicket
            {
                Cantidad = i.Cantidad.ToString(),
                Producto = i.Nombre,
                Precio = FormatoTicket.Moneda(i.PrecioUnitario),
                Subtotal = FormatoTicket.Moneda(i.SubTotal)
            }).ToList();

          
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            var dialogo = new PrintDialog();
            if (dialogo.ShowDialog() == true)
                dialogo.PrintVisual(Ticket, "Comprobante " + _factura.NumeroVenta);
        }

        private void BtnGuardarPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Comprobante_" + _factura.NumeroVenta,
                DefaultExt = ".pdf",
                Filter = "Archivo PDF (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog(this) != true) return;

            try
            {
                FacturaNegocio.GuardarPdf(_factura, dlg.FileName);
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (System.Exception ex)
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
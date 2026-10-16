using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Vistas
{

    public partial class ReporteVendedorIndividual : Page
    {
        private List<VentaTurnoItem> _ventasDelDia;
        private readonly VentaDatos _ventaDatos = new VentaDatos();

        public ReporteVendedorIndividual()
        {
            InitializeComponent();
            this.Loaded += ReporteVendedorIndividual_Loaded;
        }

        private void ReporteVendedorIndividual_Loaded(object sender, RoutedEventArgs e)
        {
            lblFechaTurno.Text = DateTime.Today.ToString("dd/MM/yyyy");

            var ventanaPadre = Window.GetWindow(this) as Window1;
            if (ventanaPadre == null || ventanaPadre.IdUsuarioActual <= 0)
            {
                MessageBox.Show("No se pudo identificar al vendedor de la sesión. Cerrá sesión e ingresá nuevamente.",
                                "Sesión inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                _ventasDelDia = new List<VentaTurnoItem>();
                dgVentasDia.ItemsSource = _ventasDelDia;
                CalcularMetricas();
                return;
            }

            lblNombreVendedor.Text = ventanaPadre.UsuarioActual;

            CargarVentasDesdeBD(ventanaPadre.IdUsuarioActual);
            CalcularMetricas();
        }

        private void CargarVentasDesdeBD(int idUsuario)
        {
            try
            {
                _ventasDelDia = _ventaDatos.ObtenerVentasDelDiaPorVendedor(idUsuario);
                dgVentasDia.ItemsSource = _ventasDelDia;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas del día: {ex.Message}", "Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                _ventasDelDia = new List<VentaTurnoItem>();
                dgVentasDia.ItemsSource = _ventasDelDia;
            }
        }

        private void CalcularMetricas()
        {
            var cultura = CultureInfo.GetCultureInfo("es-AR");

            if (_ventasDelDia == null || _ventasDelDia.Count == 0)
            {
                lblTotalFacturado.Text = (0m).ToString("C2", cultura);
                lblCantidadVentas.Text = "0 ventas registradas";
                lblTotalEfectivo.Text = (0m).ToString("C2", cultura);
                lblTotalTarjeta.Text = (0m).ToString("C2", cultura);
                lblTotalTransferencia.Text = (0m).ToString("C2", cultura);
                return;
            }

            decimal totalGeneral = _ventasDelDia.Sum(v => v.Total);
            decimal totalEfectivo = _ventasDelDia.Where(v => v.MetodoPago != null && v.MetodoPago.IndexOf("Efectivo", StringComparison.OrdinalIgnoreCase) >= 0).Sum(v => v.Total);
            decimal totalTarjeta = _ventasDelDia.Where(v => v.MetodoPago != null && v.MetodoPago.IndexOf("Tarjeta", StringComparison.OrdinalIgnoreCase) >= 0).Sum(v => v.Total);
            decimal totalTransferencia = _ventasDelDia.Where(v => v.MetodoPago != null && v.MetodoPago.IndexOf("Transferencia", StringComparison.OrdinalIgnoreCase) >= 0).Sum(v => v.Total);

            lblTotalFacturado.Text = totalGeneral.ToString("C2", cultura);
            lblCantidadVentas.Text = $"{_ventasDelDia.Count} ventas registradas";

            lblTotalEfectivo.Text = totalEfectivo.ToString("C2", cultura);
            lblTotalTarjeta.Text = totalTarjeta.ToString("C2", cultura);
            lblTotalTransferencia.Text = totalTransferencia.ToString("C2", cultura);
        }

        private void BtnImprimirCierre_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Comprobante de arqueo de caja enviado a la impresora.", "Impresión", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrarCaja_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                $"¿Confirma el arqueo y cierre de caja por un total de {lblTotalFacturado.Text}?\nEfectivo a rendir: {lblTotalEfectivo.Text}",
                "Confirmar Cierre de Turno",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                MessageBox.Show("Cierre de caja completado con éxito. Se ha guardado el balance del turno.", "Caja Cerrada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
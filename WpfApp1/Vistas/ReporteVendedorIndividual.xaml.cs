using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TiendaOga.Vistas
{
    public class VentaTurnoItem
    {
        public string NroComprobante { get; set; }
        public string Hora { get; set; }
        public string NombreCliente { get; set; }
        public string Detalle { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
    }

    public partial class ReporteVendedorIndividual : Page
    {
        private List<VentaTurnoItem> _ventasDelDia;

        public ReporteVendedorIndividual()
        {
            InitializeComponent();
            this.Loaded += ReporteVendedorIndividual_Loaded;
        }

        private void ReporteVendedorIndividual_Loaded(object sender, RoutedEventArgs e)
        {
            lblFechaTurno.Text = DateTime.Today.ToString("dd/MM/yyyy");

            var ventanaPadre = Window.GetWindow(this) as Window1;
            if (ventanaPadre != null && !string.IsNullOrWhiteSpace(ventanaPadre.UsuarioActual))
            {
                lblNombreVendedor.Text = ventanaPadre.UsuarioActual;
            }

            CargarVentasEstaticas();
            CalcularMetricas();
        }

        private void CargarVentasEstaticas()
        {
            _ventasDelDia = new List<VentaTurnoItem>
            {
                new VentaTurnoItem
                {
                    NroComprobante = "VTA-142011",
                    Hora = "09:15",
                    NombreCliente = "Carlos Benítez (DNI: 35123456)",
                    Detalle = "Taladro Percutor 650W x1",
                    MetodoPago = "Efectivo",
                    Total = 25000m
                },
                new VentaTurnoItem
                {
                    NroComprobante = "VTA-142012",
                    Hora = "10:30",
                    NombreCliente = "Mariana Romero (DNI: 28987654)",
                    Detalle = "Foco Inteligente LED Wi-Fi x2",
                    MetodoPago = "Tarjeta",
                    Total = 6400m
                },
                new VentaTurnoItem
                {
                    NroComprobante = "VTA-142013",
                    Hora = "11:45",
                    NombreCliente = "Lucía Gomez (DNI: 40112233)",
                    Detalle = "Juego de Ollas 5 piezas x1",
                    MetodoPago = "Transferencia",
                    Total = 14500m
                },
                new VentaTurnoItem
                {
                    NroComprobante = "VTA-142014",
                    Hora = "12:10",
                    NombreCliente = "Pedro Almirón (DNI: 33445566)",
                    Detalle = "Escoba de cerdas duras x2, Foco Inteligente LED x1",
                    MetodoPago = "Efectivo",
                    Total = 7200m
                }
            };

            dgVentasDia.ItemsSource = _ventasDelDia;
        }

        private void CalcularMetricas()
        {
            if (_ventasDelDia == null || _ventasDelDia.Count == 0) return;

            var cultura = CultureInfo.GetCultureInfo("es-AR");

            decimal totalGeneral = _ventasDelDia.Sum(v => v.Total);
            decimal totalEfectivo = _ventasDelDia.Where(v => v.MetodoPago == "Efectivo").Sum(v => v.Total);
            decimal totalTarjeta = _ventasDelDia.Where(v => v.MetodoPago == "Tarjeta").Sum(v => v.Total);
            decimal totalTransferencia = _ventasDelDia.Where(v => v.MetodoPago == "Transferencia").Sum(v => v.Total);

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
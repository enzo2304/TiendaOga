using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using TiendaOga.Negocio;
using TiendaOga.Entidades;

namespace TiendaOga.Vistas
{
    public partial class ReportesVendedor : Page
    {
        private bool _cargandoInicial = true; // Para evitar recargas innecesarias mientras se arma la pantalla

        public ReportesVendedor()
        {
            InitializeComponent();
            this.Loaded += ReportesVendedor_Loaded;
        }

        private void ReportesVendedor_Loaded(object sender, RoutedEventArgs e)
        {
            // Establecer fechas por defecto: Mes actual
            DateTime hoy = DateTime.Today;
            dpDesde.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dpHasta.SelectedDate = new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month));

            // Cargar combo de vendedores
            cmbVendedor.ItemsSource = RendimientoNegocio.ObtenerVendedoresActivos();

            _cargandoInicial = false; // Ya terminó de armar la pantalla

            if (cmbVendedor.Items.Count > 0)
                cmbVendedor.SelectedIndex = 0; // Esto va a disparar la carga automáticamente
        }

        private void cmbVendedor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarDatos();
        }

        private void FiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarDatos();
        }

        private void BtnAnalizar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Si la pantalla apenas está abriendo o faltan datos, no hace nada
            if (_cargandoInicial || cmbVendedor.SelectedValue == null || !dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue)
                return;

            int idVendedor = Convert.ToInt32(cmbVendedor.SelectedValue);
            DateTime desde = dpDesde.SelectedDate.Value;
            DateTime hasta = dpHasta.SelectedDate.Value;

            try
            {
                var rendimiento = RendimientoNegocio.ConsultarRendimiento(idVendedor, desde, hasta);
                ActualizarDashboard(rendimiento);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarDashboard(RendimientoVendedorDTO dto)
        {
            // 1. Llenar Tarjetas KPI
            txtFacturacion.Text = dto.VolumenFacturacion.ToString("C");
            txtVentas.Text = $"{dto.CantidadVentas} ventas concretadas";
            txtTicketPromedio.Text = dto.TicketPromedio.ToString("C");
            txtAnuladas.Text = dto.CantidadAnuladas.ToString();
            txtTasaAnulacion.Text = $"{dto.TasaAnulacion:F2}% tasa de anulación";

            // 2. Gráfico de Barras: Top Productos
            graficoTopProductos.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Unidades",
                    Values = new ChartValues<int>(dto.TopProductos.Select(p => p.Cantidad)),
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 121, 45))
                }
            };
            ejeXProductos.Labels = dto.TopProductos.Select(p => p.Nombre).ToArray();

            // 3. Gráfico de Torta: Métodos de Pago
            SeriesCollection seriesPagos = new SeriesCollection();
            foreach (var pago in dto.MetodosDePago)
            {
                seriesPagos.Add(new PieSeries
                {
                    Title = pago.Metodo,
                    Values = new ChartValues<decimal> { pago.Total },
                    DataLabels = true,
                    FontSize = 12, // Achicamos apenitas la fuente para que quede más prolija
                    // Mostrar solo el porcentaje (Ej: 45.5%). Al pasar el mouse se verá el monto total en pesos.
                    LabelPoint = chartPoint => string.Format("{0:P1}", chartPoint.Participation)
                });
            }
            graficoPagos.Series = seriesPagos;
        }
    }
}
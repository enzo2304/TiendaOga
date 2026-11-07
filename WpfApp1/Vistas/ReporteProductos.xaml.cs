using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using TiendaOga.Negocio;
using TiendaOga.Entidades;

namespace TiendaOga.Vistas
{
    public partial class ReporteProductos : Page
    {
        private bool _cargandoInicial = true;
        private ReporteGeneralDTO _datosActuales; // Guarda los datos en memoria
        private List<ReporteProductoRow> _topUnidadesCache;
        private List<ReporteProductoRow> _topRentablesCache;

        public Func<double, string> FormatoMoneda { get; set; }

        public ReporteProductos()
        {
            InitializeComponent();
            FormatoMoneda = valor => valor.ToString("C");
            DataContext = this;
            this.Loaded += ReporteProductos_Loaded;
        }

        private void ReporteProductos_Loaded(object sender, RoutedEventArgs e)
        {
            // Fechas por defecto: mes actual (igual que el reporte de vendedores)
            DateTime hoy = DateTime.Today;
            dpDesde.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dpHasta.SelectedDate = new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month));

            _cargandoInicial = false;
            CargarReporte(false);
        }

        // Cambió alguna fecha: recarga solo (sin molestar con avisos mientras se está eligiendo)
        private void FiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { CargarReporte(false); }

        // El botón sí avisa si el rango no es válido
        private void BtnGenerar_Click(object sender, RoutedEventArgs e) { CargarReporte(true); }

        private void BtnRestablecer_Click(object sender, RoutedEventArgs e)
        {
            if (_datosActuales != null) ActualizarGraficosDinamicos(null); // Null vuelve a mostrar el General
        }

        private void CargarReporte(bool mostrarAvisos)
        {
            if (_cargandoInicial) return;

            if (!dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue)
            {
                if (mostrarAvisos)
                    MessageBox.Show("Seleccioná las fechas 'Desde' y 'Hasta'.", "Fechas incompletas",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime desde = dpDesde.SelectedDate.Value;
            DateTime hasta = dpHasta.SelectedDate.Value;

            if (desde.Date > hasta.Date)
            {
                if (mostrarAvisos)
                    MessageBox.Show("La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.", "Rango inválido",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _datosActuales = ReporteNegocio.ObtenerReporteGeneral(desde, hasta);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el reporte: " + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            dgReporte.ItemsSource = _datosActuales.Productos;

            // 1. Gráfico Top Unidades
            _topUnidadesCache = _datosActuales.Productos.OrderByDescending(p => p.CantidadVendida).Take(5).ToList();
            graficoTopUnidades.Series = new SeriesCollection {
                new ColumnSeries { Title = "Unidades", Values = new ChartValues<int>(_topUnidadesCache.Select(p => p.CantidadVendida)), Fill = System.Windows.Media.Brushes.SteelBlue }
            };
            ejeXUnidades.Labels = _topUnidadesCache.Select(p => p.NombreProducto).ToArray();

            // 2. Gráfico Top Rentabilidad
            _topRentablesCache = _datosActuales.Productos.OrderByDescending(p => p.GananciaNeta).Take(5).ToList();
            graficoTopRentables.Series = new SeriesCollection {
                new ColumnSeries { Title = "Ganancia Neta", Values = new ChartValues<decimal>(_topRentablesCache.Select(p => p.GananciaNeta)), Fill = System.Windows.Media.Brushes.MediumSeaGreen }
            };
            ejeXRentables.Labels = _topRentablesCache.Select(p => p.NombreProducto).ToArray();

            ActualizarGraficosDinamicos(null);
        }

        // EVENTOS CLIC EN LOS GRÁFICOS
        private void GraficoUnidades_DataClick(object sender, ChartPoint chartPoint)
        {
            int index = (int)chartPoint.X;
            if (index >= 0 && index < _topUnidadesCache.Count)
                ActualizarGraficosDinamicos(_topUnidadesCache[index]);
        }

        private void GraficoRentables_DataClick(object sender, ChartPoint chartPoint)
        {
            int index = (int)chartPoint.X;
            if (index >= 0 && index < _topRentablesCache.Count)
                ActualizarGraficosDinamicos(_topRentablesCache[index]);
        }

        // LÓGICA QUE CAMBIA LA TORTA Y EL COMPARATIVO
        private void ActualizarGraficosDinamicos(ReporteProductoRow producto)
        {
            SeriesCollection seriesPagos = new SeriesCollection();

            if (producto == null)
            {
                // MODO GENERAL
                lblTituloTorta.Text = "Distribución de Cobranza (General)";
                lblTituloComparativo.Text = "Bruto vs Neto (Total General)";

                foreach (var pago in _datosActuales.MetodosDePagoGenerales)
                {
                    seriesPagos.Add(new PieSeries { Title = pago.Metodo, Values = new ChartValues<decimal> { pago.Total }, DataLabels = true, LabelPoint = cp => string.Format("{0:P1}", cp.Participation) });
                }

                decimal totalBruto = _datosActuales.Productos.Sum(p => p.TotalIngreso);
                decimal totalNeto = _datosActuales.Productos.Sum(p => p.GananciaNeta);

                graficoComparativo.Series = new SeriesCollection {
                    new ColumnSeries { Title = "Ingreso Bruto", Values = new ChartValues<decimal> { totalBruto }, Fill = System.Windows.Media.Brushes.LightGray },
                    new ColumnSeries { Title = "Ganancia Neta", Values = new ChartValues<decimal> { totalNeto }, Fill = System.Windows.Media.Brushes.MediumSeaGreen }
                };
                ejeXComparativo.Labels = new[] { "Todo el Catálogo" };
            }
            else
            {
                // MODO ESPECÍFICO (Al hacer clic)
                lblTituloTorta.Text = $"Cobranza: {producto.NombreProducto}";
                lblTituloComparativo.Text = $"Bruto vs Neto: {producto.NombreProducto}";

                var pagosDelProducto = _datosActuales.PagosPorProducto.Where(p => p.IdProducto == producto.IdProducto).ToList();
                foreach (var pago in pagosDelProducto)
                {
                    seriesPagos.Add(new PieSeries { Title = pago.Metodo, Values = new ChartValues<decimal> { pago.Total }, DataLabels = true, LabelPoint = cp => string.Format("{0:P1}", cp.Participation) });
                }

                graficoComparativo.Series = new SeriesCollection {
                    new ColumnSeries { Title = "Ingreso Bruto", Values = new ChartValues<decimal> { producto.TotalIngreso }, Fill = System.Windows.Media.Brushes.LightGray },
                    new ColumnSeries { Title = "Ganancia Neta", Values = new ChartValues<decimal> { producto.GananciaNeta }, Fill = System.Windows.Media.Brushes.MediumSeaGreen }
                };
                ejeXComparativo.Labels = new[] { producto.NombreProducto };
            }

            graficoPagos.Series = seriesPagos;
        }
    }
}
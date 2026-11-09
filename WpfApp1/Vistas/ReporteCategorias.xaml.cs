using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Vistas
{
    public partial class ReporteCategorias : Page
    {
        private bool _cargandoInicial = true;
        private readonly ReporteDatos _reporteDatos = new ReporteDatos();

        public ReporteCategorias()
        {
            InitializeComponent();
            this.Loaded += ReporteCategorias_Loaded;
        }

        private void ReporteCategorias_Loaded(object sender, RoutedEventArgs e)
        {
            // Inicializar rango predeterminado: desde inicio del mes actual hasta hoy
            DateTime hoy = DateTime.Now;
            dtpDesde.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dtpHasta.SelectedDate = hoy;

            _cargandoInicial = false;
            CargarReporte();
        }

        private void Filtro_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarReporte();
        }

        private void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            CargarReporte();
        }

        private void CargarReporte()
        {
            if (_cargandoInicial || dtpDesde.SelectedDate == null || dtpHasta.SelectedDate == null) return;

            DateTime desde = dtpDesde.SelectedDate.Value;
            DateTime hasta = dtpHasta.SelectedDate.Value;

            if (desde > hasta)
            {
                MessageBox.Show("La fecha 'Desde' no puede ser posterior a la fecha 'Hasta'.", "Filtro inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<ReporteCategoriaRow> lista = _reporteDatos.ObtenerReporteCategorias(desde, hasta);
            dgCategorias.ItemsSource = lista;

            // Rellenar el Gráfico de Torta con LiveCharts
            SeriesCollection series = new SeriesCollection();
            foreach (var cat in lista)
            {
                series.Add(new PieSeries
                {
                    Title = cat.NombreCategoria,
                    Values = new ChartValues<int> { cat.TotalUnidadesVendidas },
                    DataLabels = true,
                    LabelPoint = cp => string.Format("{0:P1}", cp.Participation)
                });
            }
            graficoCategorias.Series = series;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using TiendaOga.Datos;

namespace TiendaOga.Vistas
{
    public partial class ReporteGeneral : Page
    {
        private static readonly CultureInfo Ar = new CultureInfo("es-AR");

        public ReporteGeneral()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarReporte();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar el reporte general:\n" + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarReporte()
        {
            using (var cn = new ConexionBD().ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_reporte_general", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    // 1) KPIs
                    if (rd.Read())
                    {
                        txtGanancia.Text = rd.GetDecimal(0).ToString("C0", Ar);
                        txtCapital.Text = rd.GetDecimal(1).ToString("C0", Ar);
                        txtMejorVendedor.Text = rd.GetString(2);
                        txtAlertas.Text = rd.GetInt32(3).ToString();
                    }

                    // 2) Rentabilidad 6 meses
                    rd.NextResult();
                    var meses = new List<string>();
                    var ingresos = new ChartValues<decimal>();
                    var ganancias = new ChartValues<decimal>();
                    while (rd.Read())
                    {
                        string mes = rd.GetDateTime(0).ToString("MMM yy", Ar);
                        meses.Add(char.ToUpper(mes[0]) + mes.Substring(1));
                        ingresos.Add(rd.GetDecimal(1));
                        ganancias.Add(rd.GetDecimal(2));
                    }
                    graficoRentabilidad.Series = new SeriesCollection
                    {
                        new ColumnSeries { Title = "Ingresos Brutos", Values = ingresos,
                                           Fill = new SolidColorBrush(Color.FromRgb(0xE8, 0x79, 0x2D)) },
                        new ColumnSeries { Title = "Ganancia Neta", Values = ganancias,
                                           Fill = new SolidColorBrush(Color.FromRgb(0x2F, 0xA3, 0x5A)) }
                    };
                    ejeXMeses.Labels = meses;
                    ejeYRent.LabelFormatter = v => v.ToString("C0", Ar);

                    // 3) Medios de pago
                    rd.NextResult();
                    var pagos = new SeriesCollection();
                    while (rd.Read())
                    {
                        pagos.Add(new PieSeries
                        {
                            Title = rd.GetString(0),
                            Values = new ChartValues<decimal> { rd.GetDecimal(1) },
                            DataLabels = true,
                            LabelPoint = p => p.Participation.ToString("P1", Ar)
                        });
                    }
                    graficoPagos.Series = pagos;

                    // 4) Ranking de vendedores
                    rd.NextResult();
                    var vendedores = new List<FilaVendedor>();
                    while (rd.Read())
                    {
                        vendedores.Add(new FilaVendedor
                        {
                            Vendedor = rd.GetString(0),
                            CantVentas = rd.GetInt32(1).ToString(),
                            Recaudado = rd.GetDecimal(2).ToString("C0", Ar)
                        });
                    }
                    listaVendedores.ItemsSource = vendedores;
                    txtSinVendedores.Visibility = vendedores.Any() ? Visibility.Collapsed : Visibility.Visible;

                    // 5) Stock crítico
                    rd.NextResult();
                    var stock = new List<FilaStock>();
                    while (rd.Read())
                    {
                        int cant = rd.GetInt32(2);
                        stock.Add(new FilaStock
                        {
                            Producto = rd.GetString(0),
                            Categoria = rd.GetString(1),
                            Stock = cant.ToString(),
                            // rojo si está agotado, naranja si solo está bajo
                            Color = cant == 0 ? Brushes.Firebrick
                                                  : new SolidColorBrush(Color.FromRgb(0xE8, 0x79, 0x2D))
                        });
                    }
                    listaStock.ItemsSource = stock;
                    txtSinAlertas.Visibility = stock.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        private class FilaVendedor
        {
            public string Vendedor { get; set; }
            public string CantVentas { get; set; }
            public string Recaudado { get; set; }
        }

        private class FilaStock
        {
            public string Producto { get; set; }
            public string Categoria { get; set; }
            public string Stock { get; set; }
            public Brush Color { get; set; }
        }
    }
}
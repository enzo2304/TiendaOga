using System;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class ReporteProductos : Page
    {
        public ReporteProductos()
        {
            InitializeComponent();
            this.Loaded += ReporteProductos_Loaded;
        }

        private string ObtenerRolActual()
        {
            // Ajustá Window1 al nombre de tu ventana principal si es distinto
            var mainWindow = Window.GetWindow(this) as Window1;
            return mainWindow != null ? mainWindow.RolActual : string.Empty;
        }

        private void ReporteProductos_Loaded(object sender, RoutedEventArgs e)
        {
            string rol = ObtenerRolActual();

            if (!ReporteNegocio.TienePermisoReportes(rol, out string error))
            {
                MessageBox.Show(error, "Acceso Restringido", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Visibility = Visibility.Collapsed; // Oculta la pantalla
                return;
            }

            // Cargar años dinámicamente (ej: desde 2024 hasta actual)
            int anioActual = DateTime.Now.Year;
            for (int i = 2024; i <= anioActual; i++)
            {
                cmbAnio.Items.Add(i);
            }
            cmbAnio.SelectedItem = anioActual;

            // Seleccionar mes actual
            cmbMes.SelectedIndex = DateTime.Now.Month - 1;

            // Cargar grilla inicial
            CargarReporte();
        }

        private void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            CargarReporte();
        }

        private void CargarReporte()
        {
            if (cmbMes.SelectedItem is ComboBoxItem itemMes && cmbAnio.SelectedItem != null)
            {
                int mesSeleccionado = Convert.ToInt32(itemMes.Tag);
                int anioSeleccionado = Convert.ToInt32(cmbAnio.SelectedItem);

                var datos = ReporteNegocio.ObtenerProductosMasVendidos(mesSeleccionado, anioSeleccionado);
                dgReporte.ItemsSource = datos;
            }
        }
    }
}
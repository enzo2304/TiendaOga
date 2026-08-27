using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TiendaOga
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        private void btnMenuUsuarios_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pantalla de Usuarios
        }

        private void btnMenuBackUp_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pantalla de Back Up
        }

        private void btnMenuVentas_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pantalla de Ventas
        }

        private void btnMenuClientes_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pantalla de Clientes
        }

        private void btnMenuProductos_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pantalla de Productos
        }

        private void btnMenuReportes_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar pantalla de Reportes
        }

        private void btnMenuSalir_Click(object sender, RoutedEventArgs e)
        {
            // Volver al Login
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}

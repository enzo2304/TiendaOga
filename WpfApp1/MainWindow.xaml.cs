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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TiendaOga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Capturamos el usuario y contraseña
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Capturamos el rol
            ComboBoxItem itemSeleccionado = (ComboBoxItem)cmbRol.SelectedItem;
            string rol = itemSeleccionado.Content.ToString();

            // 2. Validar que no estén vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, ingrese un usuario y una contraseña.", "Datos faltantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. SIMULACIÓN DE LOS 3 ROLES
            if (usuario == "admin" && password == "1234" && rol == "Administrador")
            {
                MessageBox.Show("¡Bienvenido Administrador!", "Acceso concedido");
                Window1 ventanaDashboard = new Window1();
                ventanaDashboard.Show();
                this.Close();
            }
            else if (usuario == "enzo" && password == "1234" && rol == "Vendedor")
            {
                MessageBox.Show("¡Bienvenido Vendedor!", "Acceso concedido");
                Window1 ventanaDashboard = new Window1();
                ventanaDashboard.Show();
                this.Close();
            }
            else if (usuario == "jefe" && password == "1234" && rol == "Gerente")
            {
                MessageBox.Show("¡Bienvenido Gerente! Acceso total a reportes.", "Acceso concedido");
                Window1 ventanaDashboard = new Window1();
                ventanaDashboard.Show();
                this.Close();
            }
            else
            {
                // Si se equivoca en cualquier cosa
                MessageBox.Show("Usuario, contraseña o rol incorrectos.", "Error de acceso", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

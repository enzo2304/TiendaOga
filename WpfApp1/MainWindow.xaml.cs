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
            // 1. Capturamos lo que escribió el usuario (Trim borra espacios accidentales)
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim(); // Ojo: PasswordBox usa .Password, no .Text

            // 2. CONDICIÓN: Validar que no estén vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, ingrese un usuario y una contraseña.", "Datos faltantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // El "return" frena el código acá, no sigue leyendo hacia abajo
            }

            // 3. SIMULACIÓN DE BASE DE DATOS (Temporal)
            // Cuando conectemos la BD, reemplazaremos este "if" por un "SELECT * FROM Usuario..."
            if (usuario == "admin" && password == "1234")
            {
                // Si ingresa bien, abrimos el Dashboard
                Window1 ventanaDashboard = new Window1();
                ventanaDashboard.Show();
                this.Close(); // Cierra el login
            }
            else
            {
                // Si ingresa mal, mostramos error
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error de acceso", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

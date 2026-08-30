using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Datos; // TiendaContext

namespace TiendaOga
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuarioIngresado = txtUsuario.Text.Trim();
            string passwordIngresado = txtPassword.Password.Trim();

            if (cmbRol.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un rol.", "Rol requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string rolSeleccionado = ((ComboBoxItem)cmbRol.SelectedItem).Content.ToString();

            if (string.IsNullOrEmpty(usuarioIngresado) || string.IsNullOrEmpty(passwordIngresado))
            {
                MessageBox.Show("Por favor, complete su usuario y contraseña.", "Campos vacios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TiendaContext())
                {
                    // Traemos el usuario junto con su perfil (join automático vía navegación)
                    var usuarioEncontrado = db.Usuarios
                        .FirstOrDefault(u => u.usuario == usuarioIngresado
                                          && u.password == passwordIngresado
                                          && u.Perfil.nombre_perfil == rolSeleccionado);

                    if (usuarioEncontrado != null)
                    {
                        if (usuarioEncontrado.Activo)
                        {
                            string nombreCompleto = $"{usuarioEncontrado.nombre} {usuarioEncontrado.apellido}";
                            string rol = usuarioEncontrado.Perfil.nombre_perfil;

                            Window1 dashboard = new Window1(nombreCompleto, rol);
                            dashboard.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Tu cuenta se encuentra inactiva.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Usuario, contraseña o rol incorrectos.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos: " + ex.Message, "Error Critico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
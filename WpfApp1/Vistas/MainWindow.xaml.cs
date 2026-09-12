using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using TiendaOga.Datos;

namespace TiendaOga.Vistas
{
    public partial class MainWindow : Window
    {
        private ConexionBD conexionBD = new ConexionBD();

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
                using (SqlConnection conn = conexionBD.ObtenerConexion())
                {
                    conn.Open();

                    // Consultamos la base de datos haciendo un JOIN entre Usuario y Perfiles
                    string query = @"SELECT u.nombre, u.apellido, u.Activo, p.nombre_perfil 
                                     FROM Usuario u 
                                     INNER JOIN Perfiles p ON u.id_perfil = p.id_perfil 
                                     WHERE u.usuario = @usuario AND u.password = @password AND p.nombre_perfil = @rol";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuarioIngresado);
                        cmd.Parameters.AddWithValue("@password", passwordIngresado);
                        cmd.Parameters.AddWithValue("@rol", rolSeleccionado);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool activo = reader.GetBoolean(reader.GetOrdinal("Activo"));

                                if (activo)
                                {
                                    string nombre = reader.GetString(reader.GetOrdinal("nombre"));
                                    string apellido = reader.GetString(reader.GetOrdinal("apellido"));
                                    string rol = reader.GetString(reader.GetOrdinal("nombre_perfil"));

                                    string nombreCompleto = $"{nombre} {apellido}";

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos: " + ex.Message, "Error Critico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
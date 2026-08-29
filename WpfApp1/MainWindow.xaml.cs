using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Datos;

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

            // Validar campos vacíos
            if (string.IsNullOrEmpty(usuarioIngresado) || string.IsNullOrEmpty(passwordIngresado))
            {
                MessageBox.Show("Por favor, complete su usuario y contraseña.", "Campos vacios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = ConexionBD.ObtenerConexion())
                {
                    conn.Open();

                    // Consulta SQL según el DER (Usuario + Perfiles)
                    string query = @"SELECT u.nombre, u.apellido, p.nombre_perfil 
                                     FROM Usuario u 
                                     INNER JOIN Perfiles p ON u.id_perfil = p.id_perfil 
                                     WHERE u.usuario = @usuario 
                                       AND u.password = @pass 
                                       AND p.nombre_perfil = @rol 
                                       AND u.Activo = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuarioIngresado);
                        cmd.Parameters.AddWithValue("@pass", passwordIngresado);
                        cmd.Parameters.AddWithValue("@rol", rolSeleccionado);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string nombreCompleto = $"{reader["nombre"]} {reader["apellido"]}";
                                string rol = reader["nombre_perfil"].ToString();

                                // Abre el panel principal enviándole el nombre real y rol de la BD
                                Window1 dashboard = new Window1(nombreCompleto, rol);
                                dashboard.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Usuario, contrasena o rol incorrectos.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
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
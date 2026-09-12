using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using TiendaOga.Datos;
using TiendaOga.Negocio;

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

                    // El rol ya no se filtra por lo que elige el usuario: sale del JOIN
                    // según el id_perfil que tiene asignado en la base de datos.
                    // Tampoco se filtra por password en el WHERE: está hasheada en la
                    // base, así que no se puede comparar con "=". Se trae el hash
                    // guardado y se verifica en código con BCrypt.
                    string query = @"SELECT u.nombre, u.apellido, u.Activo, u.password, p.nombre_perfil 
                                     FROM Usuario u 
                                     INNER JOIN Perfiles p ON u.id_perfil = p.id_perfil 
                                     WHERE u.usuario = @usuario";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuarioIngresado);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hashGuardado = reader.GetString(reader.GetOrdinal("password"));

                                if (!SeguridadNegocio.VerificarPassword(passwordIngresado, hashGuardado))
                                {
                                    MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

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
                                MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
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
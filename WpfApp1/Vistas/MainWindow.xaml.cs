using System;
using System.Data;
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

                    using (SqlCommand cmd = new SqlCommand("dbo.sp_ObtenerUsuarioPorLogin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@usuario", SqlDbType.VarChar, 50).Value = usuarioIngresado;

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
                                    int idUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario"));
                                    string nombre = reader.GetString(reader.GetOrdinal("nombre"));
                                    string apellido = reader.GetString(reader.GetOrdinal("apellido"));
                                    string rol = reader.GetString(reader.GetOrdinal("nombre_perfil"));

                                    string nombreCompleto = $"{nombre} {apellido}";

                                    // Guardamos la sesión antes de abrir el dashboard,
                                    // para que cualquier vista pueda saber quién está logueado.
                                    SesionActual.IdUsuario = idUsuario;
                                    SesionActual.NombreCompleto = nombreCompleto;
                                    SesionActual.Rol = rol;

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
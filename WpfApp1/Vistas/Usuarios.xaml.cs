using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Usuarios : Page
    {
        private int? _idUsuarioSeleccionado;
        private bool _usuarioSeleccionadoActivo = true;
        private bool _sincronizandoPassword = false;
        private List<UsuarioRow> _todosLosUsuarios = new List<UsuarioRow>();

        private static readonly Regex RegexSoloLetras = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$");

        public Usuarios()
        {
            InitializeComponent();
            CargarPerfiles();
            CargarUsuarios();
        }

        private void CargarPerfiles()
        {
            cmbPerfil.ItemsSource = UsuarioNegocio.ObtenerPerfiles();
        }

        private void CargarUsuarios()
        {
            _todosLosUsuarios = UsuarioNegocio.ObtenerUsuarios().ToList();
            AplicarFiltro();
        }

        private void TxtBuscarUsuario_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string filtro = txtBuscarUsuario?.Text?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrEmpty(filtro))
            {
                dgUsuarios.ItemsSource = _todosLosUsuarios;
                return;
            }

            string[] palabras = filtro.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            dgUsuarios.ItemsSource = _todosLosUsuarios.Where(u =>
            {
                string textoCompleto = string.Join(" ", new[]
                {
                    u.Nombre, u.Apellido, u.UsuarioLogin, u.Email, u.NombrePerfil
                }).ToLower();

                return palabras.All(p => textoCompleto.Contains(p));
            }).ToList();
        }

        private void ModoOperacion_Checked(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios == null) return;

            _idUsuarioSeleccionado = null;
            _usuarioSeleccionadoActivo = true;
            dgUsuarios.SelectedItem = null;
            LimpiarFormulario();

            if (rbAlta.IsChecked == true)
            {
                lblTituloFormulario.Text = "Alta de Usuario";
                txtAyudaModo.Text = "Completá los datos para registrar un nuevo usuario.";
                btnGuardar.Content = "Registrar Usuario";
                SetFormularioHabilitado(true);
            }
            else if (rbModificar.IsChecked == true)
            {
                lblTituloFormulario.Text = "Modificar Usuario";
                txtAyudaModo.Text = "Seleccioná un usuario de la lista de abajo. Dejá la contraseña en blanco si no querés cambiarla.";
                btnGuardar.Content = "Guardar Cambios";
                SetFormularioHabilitado(true);
            }
            else if (rbBaja.IsChecked == true)
            {
                lblTituloFormulario.Text = "Dar de Baja / Reactivar Usuario";
                txtAyudaModo.Text = "Seleccioná un usuario de la lista de abajo y confirmá la acción.";
                btnGuardar.Content = "Confirmar";
                SetFormularioHabilitado(false);
            }
        }

        private void SetFormularioHabilitado(bool habilitado)
        {
            txtNombre.IsEnabled = habilitado;
            txtApellido.IsEnabled = habilitado;
            txtUsuario.IsEnabled = habilitado;
            txtPassword.IsEnabled = habilitado;
            txtPasswordVisible.IsEnabled = habilitado;
            btnMostrarPassword.IsEnabled = habilitado;
            txtEmail.IsEnabled = habilitado;
            cmbPerfil.IsEnabled = habilitado;
        }

        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbAlta.IsChecked == true) return;

            var fila = dgUsuarios.SelectedItem as UsuarioRow;
            if (fila == null) return;

            _idUsuarioSeleccionado = fila.IdUsuario;
            _usuarioSeleccionadoActivo = fila.Activo;

            txtNombre.Text = fila.Nombre;
            txtApellido.Text = fila.Apellido;
            txtUsuario.Text = fila.UsuarioLogin;
            txtEmail.Text = fila.Email;
            cmbPerfil.SelectedValue = fila.IdPerfil;
            txtPassword.Clear();
            txtPasswordVisible.Clear();

            if (rbBaja.IsChecked == true)
            {
                ActualizarTextosModoBaja();
            }
        }

        private void ActualizarTextosModoBaja()
        {
            if (_usuarioSeleccionadoActivo)
            {
                lblTituloFormulario.Text = "Dar de Baja Usuario";
                txtAyudaModo.Text = "Confirmá para desactivar al usuario seleccionado.";
                btnGuardar.Content = "Dar de Baja";
            }
            else
            {
                lblTituloFormulario.Text = "Reactivar Usuario";
                txtAyudaModo.Text = "Este usuario está inactivo. Confirmá para volver a activarlo.";
                btnGuardar.Content = "Reactivar Usuario";
            }
        }

        private void SoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RegexSoloLetras.IsMatch(e.Text);
        }

        private void SoloLetras_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string textoPegado = (string)e.DataObject.GetData(typeof(string));
                if (!RegexSoloLetras.IsMatch(textoPegado))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_sincronizandoPassword) return;
            _sincronizandoPassword = true;
            txtPasswordVisible.Text = txtPassword.Password;
            _sincronizandoPassword = false;
        }

        private void TxtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_sincronizandoPassword) return;
            _sincronizandoPassword = true;
            txtPassword.Password = txtPasswordVisible.Text;
            _sincronizandoPassword = false;
        }

        private void BtnMostrarPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPassword.Visibility = Visibility.Collapsed;
        }

        private void BtnMostrarPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (rbAlta.IsChecked == true)
            {
                RegistrarAlta();
            }
            else if (rbModificar.IsChecked == true)
            {
                GuardarModificacion();
            }
            else if (rbBaja.IsChecked == true)
            {
                ConfirmarYDarBaja();
            }
        }

        private void RegistrarAlta()
        {
            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();
            string email = txtEmail.Text.Trim();
            int? idPerfil = cmbPerfil.SelectedValue as int?;

            // Se pasa email a la validación de negocio
            if (!UsuarioNegocio.ValidarAltaUsuario(nombre, apellido, usuario, password, email, idPerfil, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos o inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UsuarioNegocio.AltaUsuario(nombre, apellido, usuario, password, email, idPerfil.Value);

            MessageBox.Show("Usuario registrado correctamente.", "Alta exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

            LimpiarFormulario();
            CargarUsuarios();
        }

        private void GuardarModificacion()
        {
            if (_idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un usuario de la lista para modificar.", "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();
            string email = txtEmail.Text.Trim();
            int? idPerfil = cmbPerfil.SelectedValue as int?;

            // Se pasa email a la validación de modificación
            if (!UsuarioNegocio.ValidarModificacionUsuario(nombre, apellido, usuario, email, idPerfil, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos o inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Estás seguro de modificar los datos del usuario \"{usuario}\"?",
                "Confirmar modificación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            UsuarioNegocio.ModificarUsuario(_idUsuarioSeleccionado.Value, nombre, apellido, usuario, password, email, idPerfil.Value);

            MessageBox.Show("Usuario modificado correctamente.", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

            _idUsuarioSeleccionado = null;
            LimpiarFormulario();
            CargarUsuarios();
        }

        private void ConfirmarYDarBaja()
        {
            if (_idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un usuario de la lista.", "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool vaAReactivar = !_usuarioSeleccionadoActivo;

            string pregunta = vaAReactivar
                ? $"¿Estás seguro de reactivar al usuario \"{txtUsuario.Text}\"?"
                : $"¿Estás seguro de dar de baja al usuario \"{txtUsuario.Text}\"?";

            var resultado = MessageBox.Show(pregunta, "Confirmar acción", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            if (vaAReactivar)
            {
                UsuarioNegocio.ReactivarUsuario(_idUsuarioSeleccionado.Value);
                MessageBox.Show("Usuario reactivado correctamente.", "Reactivación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                UsuarioNegocio.DarBajaUsuario(_idUsuarioSeleccionado.Value);
                MessageBox.Show("Usuario dado de baja correctamente.", "Baja exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _idUsuarioSeleccionado = null;
            _usuarioSeleccionadoActivo = true;
            LimpiarFormulario();
            CargarUsuarios();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _idUsuarioSeleccionado = null;
            _usuarioSeleccionadoActivo = true;
            dgUsuarios.SelectedItem = null;
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtApellido.Clear();
            txtUsuario.Clear();
            txtPassword.Clear();
            txtPasswordVisible.Clear();
            txtEmail.Clear();
            cmbPerfil.SelectedIndex = -1;
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Usuarios : Page
    {
        private int? _idUsuarioSeleccionado;
        private bool _usuarioSeleccionadoActivo = true;

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
            dgUsuarios.ItemsSource = UsuarioNegocio.ObtenerUsuarios();
        }

        // ==========================================================
        // Cambio de modo: Alta / Modificar / Dar de Baja
        // ==========================================================

        private void ModoOperacion_Checked(object sender, RoutedEventArgs e)
        {
            // Los controles todavía pueden no existir mientras se arma
            // el XAML (InitializeComponent dispara Checked del RadioButton
            // marcado por defecto antes de terminar de crear el resto de
            // la pantalla). dgUsuarios está en la segunda tarjeta, así que
            // es de los últimos controles en crearse: si todavía es null,
            // significa que la ventana no terminó de armarse.
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
                // Textos por defecto (sin selección todavía). Se ajustan
                // en DgUsuarios_SelectionChanged según el estado real
                // del usuario que se seleccione.
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
            txtEmail.IsEnabled = habilitado;
            cmbPerfil.IsEnabled = habilitado;
        }

        // ==========================================================
        // Selección de fila -> cargar formulario (Modificar / Baja)
        // ==========================================================

        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbAlta.IsChecked == true) return; // en Alta no se edita un usuario existente

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

            if (rbBaja.IsChecked == true)
            {
                ActualizarTextosModoBaja();
            }
        }

        /// <summary>
        /// En modo Baja, el título/ayuda/botón cambian según si el
        /// usuario seleccionado está activo (se puede dar de baja) o
        /// inactivo (se puede reactivar).
        /// </summary>
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

        // ==========================================================
        // Guardar: se comporta distinto según el modo activo
        // ==========================================================

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

            if (!UsuarioNegocio.ValidarAltaUsuario(nombre, apellido, usuario, password, idPerfil, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            string password = txtPassword.Password.Trim(); // vacío = no cambiar la contraseña
            string email = txtEmail.Text.Trim();
            int? idPerfil = cmbPerfil.SelectedValue as int?;

            if (!UsuarioNegocio.ValidarModificacionUsuario(nombre, apellido, usuario, idPerfil, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            txtEmail.Clear();
            cmbPerfil.SelectedIndex = -1;
        }
    }
}
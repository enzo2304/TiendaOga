using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Usuarios : Page
    {
        private int? _idUsuarioSeleccionado;
        private bool _usuarioSeleccionadoActivo = true;
        private int? _idPerfilOriginalSeleccionado;
        private bool _sincronizandoPassword = false;
        private List<UsuarioRow> _todosLosUsuarios = new List<UsuarioRow>();
        private List<UsuarioRow> _usuariosFiltrados = new List<UsuarioRow>();

        private const int TAMANIO_PAGINA = 10;
        private int _paginaActual = 1;

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
            _paginaActual = 1;
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string filtro = txtBuscarUsuario?.Text ?? string.Empty;
            _usuariosFiltrados = UsuarioNegocio.FiltrarUsuarios(_todosLosUsuarios, filtro);
            RenderizarPagina();
        }

        private void RenderizarPagina()
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(_usuariosFiltrados.Count / (double)TAMANIO_PAGINA));

            if (_paginaActual > totalPaginas) _paginaActual = totalPaginas;
            if (_paginaActual < 1) _paginaActual = 1;

            dgUsuarios.ItemsSource = _usuariosFiltrados
                .Skip((_paginaActual - 1) * TAMANIO_PAGINA)
                .Take(TAMANIO_PAGINA)
                .ToList();

            RenderizarPaginacion(totalPaginas);
        }

        private void RenderizarPaginacion(int totalPaginas)
        {
            pnlPaginacion.Children.Clear();

            if (_usuariosFiltrados.Count <= TAMANIO_PAGINA)
                return;

            pnlPaginacion.Children.Add(CrearBotonPaginacion("‹ Previo", _paginaActual > 1, () =>
            {
                _paginaActual--;
                RenderizarPagina();
            }));

            var numerosAMostrar = ObtenerNumerosDePagina(_paginaActual, totalPaginas);

            foreach (var numero in numerosAMostrar)
            {
                if (numero == null)
                {
                    var puntos = new TextBlock
                    {
                        Text = "...",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(6, 0, 6, 0),
                        Foreground = (Brush)new BrushConverter().ConvertFrom("#718096")
                    };
                    pnlPaginacion.Children.Add(puntos);
                }
                else
                {
                    int pagina = numero.Value;
                    bool esActual = pagina == _paginaActual;

                    var boton = new Button
                    {
                        Content = pagina.ToString(),
                        Width = 32,
                        Height = 32,
                        Margin = new Thickness(2, 0, 2, 0),
                        Background = esActual ? (Brush)new BrushConverter().ConvertFrom("#FF6B00") : Brushes.White,
                        Foreground = esActual ? Brushes.White : (Brush)new BrushConverter().ConvertFrom("#4A5568"),
                        BorderBrush = (Brush)new BrushConverter().ConvertFrom("#CBD5E0"),
                        BorderThickness = new Thickness(1),
                        FontWeight = esActual ? FontWeights.Bold : FontWeights.Normal,
                        Cursor = Cursors.Hand,
                        IsEnabled = !esActual
                    };

                    boton.Click += (s, e) =>
                    {
                        _paginaActual = pagina;
                        RenderizarPagina();
                    };

                    pnlPaginacion.Children.Add(boton);
                }
            }

            pnlPaginacion.Children.Add(CrearBotonPaginacion("Siguiente ›", _paginaActual < totalPaginas, () =>
            {
                _paginaActual++;
                RenderizarPagina();
            }));
        }

        private Button CrearBotonPaginacion(string texto, bool habilitado, Action alHacerClick)
        {
            var boton = new Button
            {
                Content = texto,
                Height = 32,
                Padding = new Thickness(10, 0, 10, 0),
                Margin = new Thickness(2, 0, 2, 0),
                Background = Brushes.White,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#4A5568"),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#CBD5E0"),
                BorderThickness = new Thickness(1),
                IsEnabled = habilitado,
                Cursor = habilitado ? Cursors.Hand : Cursors.Arrow
            };

            boton.Click += (s, e) => alHacerClick();
            return boton;
        }

        private List<int?> ObtenerNumerosDePagina(int paginaActual, int totalPaginas)
        {
            var resultado = new List<int?>();

            if (totalPaginas <= 5)
            {
                for (int i = 1; i <= totalPaginas; i++) resultado.Add(i);
                return resultado;
            }

            resultado.Add(1);

            if (paginaActual > 3)
                resultado.Add(null);

            int inicio = Math.Max(2, paginaActual - 1);
            int fin = Math.Min(totalPaginas - 1, paginaActual + 1);

            for (int i = inicio; i <= fin; i++)
                resultado.Add(i);

            if (paginaActual < totalPaginas - 2)
                resultado.Add(null);

            resultado.Add(totalPaginas);

            return resultado;
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
            _idPerfilOriginalSeleccionado = fila.IdPerfil;

            txtNombre.Text = fila.Nombre;
            txtApellido.Text = fila.Apellido;
            txtUsuario.Text = fila.UsuarioLogin;
            txtEmail.Text = fila.Email;
            cmbPerfil.SelectedValue = fila.IdPerfil;
            txtPassword.Clear();
            txtPasswordVisible.Clear();

            bool esElUsuarioLogueado = fila.IdUsuario == SesionActual.IdUsuario;
            if (rbModificar.IsChecked == true)
            {
                cmbPerfil.IsEnabled = !esElUsuarioLogueado;
                txtAyudaModo.Text = esElUsuarioLogueado
                    ? "Estás editando tu propio usuario: no podés cambiarte el perfil vos mismo para preservar tu acceso."
                    : "Seleccioná un usuario de la lista de abajo. Dejá la contraseña en blanco si no querés cambiarla.";
            }

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

            if (!UsuarioNegocio.ValidarAltaUsuario(nombre, apellido, usuario, password, email, idPerfil, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos o inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                UsuarioNegocio.AltaUsuario(nombre, apellido, usuario, password, email, idPerfil.Value);
                MessageBox.Show("Usuario registrado correctamente.", "Alta exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                CargarUsuarios();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error inesperado al registrar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (!UsuarioNegocio.ValidarModificacionUsuario(
                _idUsuarioSeleccionado.Value,
                SesionActual.IdUsuario,
                _idPerfilOriginalSeleccionado,
                nombre,
                apellido,
                usuario,
                email,
                idPerfil,
                out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Estás seguro de modificar los datos del usuario \"{usuario}\"?",
                "Confirmar modificación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                UsuarioNegocio.ModificarUsuario(_idUsuarioSeleccionado.Value, nombre, apellido, usuario, password, email, idPerfil.Value);
                MessageBox.Show("Usuario modificado correctamente.", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                _idUsuarioSeleccionado = null;
                LimpiarFormulario();
                CargarUsuarios();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error inesperado al modificar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfirmarYDarBaja()
        {
            if (_idUsuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un usuario de la lista.", "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool vaAReactivar = !_usuarioSeleccionadoActivo;

            if (!vaAReactivar && !UsuarioNegocio.ValidarBajaUsuario(_idUsuarioSeleccionado.Value, SesionActual.IdUsuario, out string mensajeErrorBaja))
            {
                MessageBox.Show(mensajeErrorBaja, "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string pregunta = vaAReactivar
                ? $"¿Estás seguro de reactivar al usuario \"{txtUsuario.Text}\"?"
                : $"¿Estás seguro de dar de baja al usuario \"{txtUsuario.Text}\"?";

            var resultado = MessageBox.Show(pregunta, "Confirmar acción", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
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
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error inesperado al procesar la baja/reactivación: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _idUsuarioSeleccionado = null;
            _usuarioSeleccionadoActivo = true;
            _idPerfilOriginalSeleccionado = null;
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
            cmbPerfil.IsEnabled = true;
        }
    }
}
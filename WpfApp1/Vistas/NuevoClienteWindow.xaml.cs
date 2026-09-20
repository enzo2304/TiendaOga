using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class NuevoClienteWindow : Window
    {
        public ClienteItem ClienteCreado { get; private set; }

        private readonly ClienteItem _clienteEnEdicion;
        private static readonly Regex RegexSoloNumeros = new Regex(@"^[0-9]+$");

        public NuevoClienteWindow()
        {
            InitializeComponent();
        }

        public NuevoClienteWindow(ClienteItem clienteExistente)
        {
            InitializeComponent();
            _clienteEnEdicion = clienteExistente;

            Title = "Editar Cliente";
            txtTitulo.Text = "Modificar Datos del Cliente";
            btnAceptar.Content = "Guardar Cambios";

            txtNombre.Text = clienteExistente.NombreCompleto;
            txtNroDocumento.Text = clienteExistente.Dni;
            txtTelefono.Text = clienteExistente.Telefono;
            chkActivo.IsChecked = clienteExistente.Activo;

            SeleccionarPorContenido(cmbTipoCliente, clienteExistente.TipoCliente);
            SeleccionarPorContenido(cmbTipoDocumento, clienteExistente.TipoDocumento);
        }

        private void SeleccionarPorContenido(ComboBox combo, string valor)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (string.Equals(item.Content?.ToString(), valor, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RegexSoloNumeros.IsMatch(e.Text);
        }

        private void SoloNumeros_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string textoPegado = (string)e.DataObject.GetData(typeof(string));
                if (!RegexSoloNumeros.IsMatch(textoPegado))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text?.Trim() ?? string.Empty;
            string nroDocumento = txtNroDocumento.Text?.Trim() ?? string.Empty;
            string telefono = txtTelefono.Text?.Trim() ?? string.Empty;
            string tipoCliente = (cmbTipoCliente.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string tipoDocumento = (cmbTipoDocumento.SelectedItem as ComboBoxItem)?.Content?.ToString();
            bool activo = chkActivo.IsChecked == true;

            if (_clienteEnEdicion == null)
            {
                // Modo Alta
                if (!ClienteNegocio.ValidarAltaCliente(nombre, nroDocumento, telefono, out string mensajeError))
                {
                    MessageBox.Show(mensajeError, "Dato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirmacion = MessageBox.Show(
                    $"¿Está seguro que desea agregar al cliente \"{nombre}\"?",
                    "Confirmar alta de cliente",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacion != MessageBoxResult.Yes)
                    return;

                ClienteCreado = ClienteNegocio.CrearCliente(nombre, nroDocumento, telefono, tipoCliente, tipoDocumento, activo);
            }
            else
            {
                // Modo Edición
                if (!ClienteNegocio.ValidarEdicionCliente(_clienteEnEdicion.IdCliente, nombre, nroDocumento, telefono, out string mensajeError))
                {
                    MessageBox.Show(mensajeError, "Dato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirmacion = MessageBox.Show(
                    $"¿Está seguro que desea guardar los cambios del cliente \"{nombre}\"?",
                    "Confirmar modificación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacion != MessageBoxResult.Yes)
                    return;

                // Delegación de la mutación a la capa de negocio
                ClienteNegocio.ModificarCliente(_clienteEnEdicion, nombre, nroDocumento, telefono, tipoCliente, tipoDocumento, activo);
                ClienteCreado = _clienteEnEdicion;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
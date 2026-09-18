using System;
using System.Linq;
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

        private static readonly Regex RegexSoloNumeros = new Regex(@"^[0-9]+$");

        public NuevoClienteWindow()
        {
            InitializeComponent();
        }

        // Bloquea cualquier tecla que no sea un número del 0 al 9
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RegexSoloNumeros.IsMatch(e.Text);
        }

        // Evita que peguen texto con letras o símbolos
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
            string nombre = txtNombre.Text.Trim();
            string nroDocumento = txtNroDocumento.Text.Trim();
            string telefono = txtTelefono.Text.Trim();

            // Llamada a la capa de Negocio
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

            string tipoCliente = (cmbTipoCliente.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string tipoDocumento = (cmbTipoDocumento.SelectedItem as ComboBoxItem)?.Content?.ToString();

            ClienteCreado = ClienteNegocio.CrearCliente(nombre, nroDocumento, telefono, tipoCliente, tipoDocumento, chkActivo.IsChecked == true);

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
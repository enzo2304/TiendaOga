using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;

namespace TiendaOga.Vistas
{
    public partial class NuevoClienteWindow : Window
    {
        public ClienteItem ClienteCreado { get; private set; }

        public NuevoClienteWindow()
        {
            InitializeComponent();
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string nroDocumento = txtNroDocumento.Text.Trim();
            string telefono = txtTelefono.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(nroDocumento))
            {
                MessageBox.Show("Por favor completá el Nombre y el Nro. de Documento.",
                    "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro que desea agregar al cliente \"{nombre}\"?",
                "Confirmar alta de cliente",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            ClienteCreado = new ClienteItem
            {
                IdCliente = DatosGlobales.Clientes.Count == 0
                    ? 1
                    : System.Linq.Enumerable.Max(System.Linq.Enumerable.Select(DatosGlobales.Clientes, c => c.IdCliente)) + 1,
                TipoCliente = (cmbTipoCliente.SelectedItem as ComboBoxItem)?.Content.ToString(),
                NombreCompleto = nombre,
                TipoDocumento = (cmbTipoDocumento.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Dni = nroDocumento,
                Telefono = string.IsNullOrWhiteSpace(telefono) ? "S/D" : telefono,
                Activo = chkActivo.IsChecked == true
            };

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
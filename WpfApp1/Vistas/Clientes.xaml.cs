using System;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Clientes : Page
    {
        public Clientes()
        {
            InitializeComponent();
            this.Loaded += Clientes_Loaded;
        }

        private void Clientes_Loaded(object sender, RoutedEventArgs e)
        {
            CargarListaClientes();
        }

        private void CargarListaClientes()
        {
            dgClientes.ItemsSource = null;
            dgClientes.ItemsSource = ClienteNegocio.ObtenerTodos();
        }

        private void DgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cliente = dgClientes.SelectedItem as ClienteItem;
            if (cliente != null)
            {
                lblHistorialTitulo.Text = $"Historial de Compras: {cliente.NombreCompleto} (DNI: {cliente.Dni})";
                dgCompras.ItemsSource = cliente.HistorialCompras;
            }
            else
            {
                lblHistorialTitulo.Text = "Historial de Compras (Seleccione un cliente para ver qué compró)";
                dgCompras.ItemsSource = null;
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            dgClientes.ItemsSource = ClienteNegocio.BuscarClientes(txtBuscar.Text);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Clear();
            CargarListaClientes();
        }

        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new NuevoClienteWindow
            {
                Owner = Window.GetWindow(this)
            };

            bool? resultado = ventana.ShowDialog();

            if (resultado == true && ventana.ClienteCreado != null)
            {
                ClienteNegocio.AgregarCliente(ventana.ClienteCreado);
                CargarListaClientes();
            }
        }

        private void BtnModificarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is ClienteItem cliente)
            {
                var ventana = new NuevoClienteWindow(cliente)
                {
                    Owner = Window.GetWindow(this)
                };

                bool? resultado = ventana.ShowDialog();
                if (resultado == true)
                {
                    CargarListaClientes();
                }
            }
        }

        private void BtnToggleActivo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is ClienteItem cliente)
            {
                bool vaAActivar = !cliente.Activo;
                string accion = vaAActivar ? "reactivar" : "dar de baja a";

                var confirmacion = MessageBox.Show(
                    $"¿Está seguro que desea {accion} al cliente \"{cliente.NombreCompleto}\"?",
                    vaAActivar ? "Confirmar reactivación" : "Confirmar baja de cliente",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacion != MessageBoxResult.Yes)
                    return;

                ClienteNegocio.CambiarEstadoActivo(cliente.IdCliente, vaAActivar);
                dgClientes.Items.Refresh();
            }
        }
    }
}
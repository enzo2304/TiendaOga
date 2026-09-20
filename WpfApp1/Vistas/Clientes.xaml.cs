using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;

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
            dgClientes.ItemsSource = DatosGlobales.Clientes;
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
            string busqueda = txtBuscar.Text?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(busqueda))
            {
                dgClientes.ItemsSource = DatosGlobales.Clientes;
            }
            else
            {
                dgClientes.ItemsSource = DatosGlobales.Clientes
                    .Where(c => c.NombreCompleto.ToLower().Contains(busqueda) || c.Dni.Contains(busqueda))
                    .ToList();
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Clear();
            dgClientes.ItemsSource = DatosGlobales.Clientes;
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
                DatosGlobales.Clientes.Insert(0, ventana.ClienteCreado);
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

                // No hace falta reasignar nada al aceptar: la ventana edita
                // el mismo objeto "cliente" que ya está dentro de la colección,
                // así que los cambios se ven solos gracias a INotifyPropertyChanged.
                ventana.ShowDialog();
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

                cliente.Activo = vaAActivar;
            }
        }
    }
}
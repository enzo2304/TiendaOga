using System;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;
using TiendaOga.Negocio;
using System.Linq;

namespace TiendaOga.Vistas
{
    public partial class Clientes : Page
    {
        private const string TITULO_HISTORIAL_VACIO = "Historial de Compras (Seleccione un cliente para ver qué compró)";

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
            try
            {
                dgClientes.ItemsSource = null;
                dgClientes.ItemsSource = ClienteNegocio.ObtenerTodos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar la lista de clientes: " + ex.Message,
                    "Error de base de datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cliente = dgClientes.SelectedItem as ClienteItem;

            if (cliente == null)
            {
                lblHistorialTitulo.Text = TITULO_HISTORIAL_VACIO;
                dgCompras.ItemsSource = null;
                return;
            }

            lblHistorialTitulo.Text = $"Historial de Compras: {cliente.NombreCompleto} ({cliente.TipoDocumento}: {cliente.Dni})";

            try
            {
                dgCompras.ItemsSource = ClienteNegocio.ObtenerHistorial(cliente.IdCliente);
            }
            catch (Exception ex)
            {
                dgCompras.ItemsSource = null;
                MessageBox.Show("No se pudo cargar el historial de compras: " + ex.Message,
                    "Error de base de datos", MessageBoxButton.OK, MessageBoxImage.Error);
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
                try
                {
                    ClienteNegocio.AgregarCliente(ventana.ClienteCreado);
                    CargarListaClientes();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error inesperado al registrar el cliente: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

                try
                {
                    ClienteNegocio.CambiarEstadoActivo(cliente.IdCliente, vaAActivar);
                    dgClientes.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cambiar el estado del cliente: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BtnVerFactura_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is int idVenta)
            {
                try
                {
                    new ComprobanteVentaWindow(idVenta) { Owner = Window.GetWindow(this) }.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo abrir el comprobante:\n" + ex.Message, "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnAnularVenta_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button boton) || !(boton.Tag is int idVenta)) return;

            var ventanaPadre = Window.GetWindow(this) as Window1;
            if (ventanaPadre == null || ventanaPadre.IdUsuarioActual <= 0)
            {
                MessageBox.Show("No se pudo identificar al usuario de la sesión.", "Sesión inválida",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Anular esta venta? Se devolverá el stock y no se puede deshacer.",
                "Confirmar anulación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                // Guardo el cliente seleccionado: al recargar la lista se pierde la selección
                int idClienteSel = (dgClientes.SelectedItem as ClienteItem)?.IdCliente ?? 0;

                VentaNegocio.AnularVenta(idVenta, ventanaPadre.IdUsuarioActual);

                // TODO 1: recargar clientes (actualiza "Compras Realizadas")
                CargarListaClientes();

                // TODO 2: reseleccionar el cliente; esto dispara DgClientes_SelectionChanged,
                // que vuelve a cargar dgCompras
                if (idClienteSel > 0)
                {
                    dgClientes.SelectedItem = DatosGlobales.Clientes
                        .FirstOrDefault(c => c.IdCliente == idClienteSel);
                }

                MessageBox.Show("Venta anulada correctamente.", "Listo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                MessageBox.Show(ex.Message, "No se pudo anular",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al anular la venta: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


}
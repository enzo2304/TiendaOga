using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Ventas : Page
    {
        private readonly ObservableCollection<ItemVenta> _detalleVenta = new ObservableCollection<ItemVenta>();
        private static readonly Regex RegexSoloNumeros = new Regex(@"^[0-9]+$");
        private bool _sincronizandoProducto = false;

        public Ventas()
        {
            InitializeComponent();
            dgDetalleVenta.ItemsSource = _detalleVenta;
            dpFechaPago.SelectedDate = DateTime.Today;

            // Recarga automática al ingresar o regresar a la pantalla de Ventas
            this.Loaded += Ventas_Loaded;

            ActualizarEstadoPago();
        }

        private void Ventas_Loaded(object sender, RoutedEventArgs e)
        {
            CargarClientes();
            CargarListaProductos();
        }

        private void CargarClientes()
        {
            if (cmbCliente == null) return;

            cmbCliente.ItemsSource = null;
            cmbCliente.ItemsSource = DatosGlobales.Clientes;

            if (DatosGlobales.Clientes != null && DatosGlobales.Clientes.Count > 0)
            {
                cmbCliente.SelectedIndex = 0;
            }
        }

        private void CargarListaProductos()
        {
            cmbProductoBusqueda.ItemsSource = null;
            cmbProductoBusqueda.ItemsSource = DatosGlobales.Productos;
        }

        // ==========================================================
        // FILTRADO DE TECLADO
        // ==========================================================

        private void TxtCodigoProducto_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RegexSoloNumeros.IsMatch(e.Text);
        }

        private void TxtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RegexSoloNumeros.IsMatch(e.Text);
        }

        // ==========================================================
        // SINCRONIZACIÓN: BÚSQUEDA POR CÓDIGO (ID) Y POR COMBOBOX
        // ==========================================================

        private void TxtCodigoProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                BuscarPorCodigo();
                e.Handled = true;
            }
        }

        private void BuscarPorCodigo()
        {
            if (string.IsNullOrWhiteSpace(txtCodigoProducto.Text)) return;

            if (int.TryParse(txtCodigoProducto.Text.Trim(), out int idBuscado))
            {
                var prod = DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == idBuscado);
                if (prod != null)
                {
                    _sincronizandoProducto = true;
                    cmbProductoBusqueda.SelectedItem = prod;
                    txtPrecioUnitario.Text = prod.PrecioVentas.ToString("N2", CultureInfo.InvariantCulture);
                    _sincronizandoProducto = false;
                    txtCantidad.Focus();
                }
                else
                {
                    MessageBox.Show($"No se encontró ningún producto con el código {idBuscado}.", "Producto no encontrado", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCodigoProducto.SelectAll();
                    txtCodigoProducto.Focus();
                }
            }
        }

        private void CmbProductoBusqueda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_sincronizandoProducto) return;

            if (cmbProductoBusqueda.SelectedItem is ProductoRow producto)
            {
                _sincronizandoProducto = true;
                txtCodigoProducto.Text = producto.IdProducto.ToString();
                txtPrecioUnitario.Text = producto.PrecioVentas.ToString("N2", CultureInfo.InvariantCulture);
                _sincronizandoProducto = false;

                txtCantidad.Focus();
            }
            else
            {
                txtCodigoProducto.Clear();
                txtPrecioUnitario.Clear();
            }
        }

        // ==========================================================
        // DETALLE DE VENTA: AGREGAR / ELIMINAR
        // ==========================================================

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = cmbProductoBusqueda.SelectedItem as ProductoRow;

            // Si tipeó el código pero no seleccionó en el combo
            if (productoSeleccionado == null && !string.IsNullOrWhiteSpace(txtCodigoProducto.Text))
            {
                if (int.TryParse(txtCodigoProducto.Text.Trim(), out int id))
                {
                    productoSeleccionado = DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == id);
                }
            }

            if (productoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un producto del catálogo o ingresá un código válido.", "Datos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigoProducto.Focus();
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresá una cantidad numérica mayor a 0.", "Cantidad inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCantidad.Focus();
                return;
            }

            // Validar stock disponible considerando lo que ya esté cargado en la grilla
            int cantidadYaAgregada = _detalleVenta
                .Where(i => i.IdProducto == productoSeleccionado.IdProducto)
                .Sum(i => i.Cantidad);

            if (!VentaNegocio.ValidarStockDisponible(productoSeleccionado.Stock, cantidadYaAgregada, cantidad, out string errorStock))
            {
                MessageBox.Show(errorStock, "Stock no disponible", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si el ítem ya existe en la lista sumamos la cantidad, sino agregamos una fila nueva
            var itemExistente = _detalleVenta.FirstOrDefault(i => i.IdProducto == productoSeleccionado.IdProducto);
            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
                dgDetalleVenta.Items.Refresh();
            }
            else
            {
                _detalleVenta.Add(new ItemVenta
                {
                    IdProducto = productoSeleccionado.IdProducto,
                    Nombre = productoSeleccionado.NombreProducto,
                    PrecioVenta = productoSeleccionado.PrecioVentas,
                    Cantidad = cantidad
                });
            }

            // Limpiar campos de selección rápida
            _sincronizandoProducto = true;
            txtCodigoProducto.Clear();
            cmbProductoBusqueda.SelectedIndex = -1;
            txtPrecioUnitario.Clear();
            txtCantidad.Text = "1";
            _sincronizandoProducto = false;

            ActualizarTotal();
            txtCodigoProducto.Focus();
        }

        private void BtnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is ItemVenta item)
            {
                _detalleVenta.Remove(item);
                ActualizarTotal();
            }
        }

        // ==========================================================
        // CÁLCULO DE TOTALES Y PAGO
        // ==========================================================

        private void ActualizarTotal()
        {
            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);
            txtPrecioTotal.Text = total.ToString("C2", CultureInfo.GetCultureInfo("es-AR"));
            ActualizarEstadoPago();
        }

        private void CmbTipoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarEstadoPago();
        }

        private void TxtMontoRecibido_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularVuelto();
        }

        private void ActualizarEstadoPago()
        {
            if (cmbTipoPago == null || txtMontoRecibido == null || txtVuelto == null) return;

            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);

            if (string.IsNullOrWhiteSpace(txtMontoRecibido.Text) || txtMontoRecibido.Text == "0.00")
            {
                txtMontoRecibido.Text = total.ToString("N2", CultureInfo.InvariantCulture);
            }

            CalcularVuelto();
        }

        private void CalcularVuelto()
        {
            if (txtMontoRecibido == null || txtVuelto == null) return;

            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);
            decimal.TryParse(txtMontoRecibido.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal recibido);

            decimal vuelto = VentaNegocio.CalcularVuelto(total, recibido);

            txtVuelto.Text = vuelto.ToString("C2", CultureInfo.GetCultureInfo("es-AR"));
            txtVuelto.Foreground = vuelto < 0
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.Black;
        }

        // ==========================================================
        // GUARDAR VENTA
        // ==========================================================

        private void BtnGuardarVenta_Click(object sender, RoutedEventArgs e)
        {
            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);
            decimal.TryParse(txtMontoRecibido.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal recibido);

            if (!VentaNegocio.ValidarVenta(_detalleVenta.Count, total, recibido, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "No se puede guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar selección de cliente
            var clienteSeleccionado = cmbCliente.SelectedItem as ClienteItem;
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Por favor seleccioná un cliente registrado para la venta.", "Cliente requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCliente.Focus();
                return;
            }

            // 1. Descontar stock de los productos vendidos en memoria
            foreach (var item in _detalleVenta)
            {
                var prod = DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == item.IdProducto);
                if (prod != null)
                {
                    prod.Stock -= item.Cantidad;
                }
            }

            // 2. Registrar compra en el padrón de clientes
            var listaNombres = _detalleVenta.Select(item => $"{item.Nombre} x{item.Cantidad}").ToList();
            string detalleProductos = string.Join(", ", listaNombres);

            string metodoPago = cmbTipoPago.SelectedItem is ComboBoxItem itemCombo
                ? itemCombo.Content.ToString()
                : cmbTipoPago.Text;

            DateTime fechaPago = dpFechaPago.SelectedDate ?? DateTime.Today;

            DatosGlobales.RegistrarCompraCliente(
                nombre: clienteSeleccionado.NombreCompleto,
                dni: clienteSeleccionado.Dni,
                telefono: clienteSeleccionado.Telefono,
                detalle: detalleProductos,
                total: total,
                metodoPago: metodoPago,
                fecha: fechaPago
            );

            MessageBox.Show("¡Venta registrada con éxito! El stock y el historial del cliente fueron actualizados.", "Venta Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

            // Refrescar catálogo para mostrar el stock restante actualizado en el ComboBox
            CargarListaProductos();
            BtnCancelarVenta_Click(sender, e);
        }

        private void BtnCancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            _detalleVenta.Clear();

            if (DatosGlobales.Clientes != null && DatosGlobales.Clientes.Count > 0)
            {
                cmbCliente.SelectedIndex = 0;
            }

            _sincronizandoProducto = true;
            txtCodigoProducto.Clear();
            cmbProductoBusqueda.SelectedIndex = -1;
            txtPrecioUnitario.Clear();
            txtCantidad.Text = "1";
            _sincronizandoProducto = false;

            cmbTipoPago.SelectedIndex = 0;
            dpFechaPago.SelectedDate = DateTime.Today;
            ActualizarTotal();
        }
    }
}
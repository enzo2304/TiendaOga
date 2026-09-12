using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    /// <summary>
    /// Vista de Nueva Venta. Contiene ÚNICAMENTE lógica de interfaz:
    /// alta/baja de ítems en la grilla (en memoria) y refresco de pantalla.
    /// El cálculo y la validación viven en TiendaOga.Negocio.VentaNegocio.
    /// NO incluye persistencia contra base de datos (queda fuera de alcance).
    /// </summary>
    public partial class Ventas : Page
    {
        private readonly ObservableCollection<ItemVenta> _detalleVenta = new ObservableCollection<ItemVenta>();

        public Ventas()
        {
            InitializeComponent();
            dgDetalleVenta.ItemsSource = _detalleVenta;
            ActualizarEstadoPago();
        }

        // ==========================================================
        // Búsqueda de cliente (placeholder de UI, sin conexión a BD)
        // ==========================================================

        private void BtnBuscarCliente_Click(object sender, RoutedEventArgs e)
        {
            // Fuera de alcance: acá iría la búsqueda del cliente.
        }

        // ==========================================================
        // Alta / baja de ítems en la grilla de detalle (en memoria)
        // ==========================================================

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtProductoBusqueda.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Ingresá un producto antes de agregar.", "Datos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal precioUnitario;
            if (!decimal.TryParse(txtPrecioUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out precioUnitario))
            {
                precioUnitario = 0;
            }

            int cantidad;
            int.TryParse(txtCantidad.Text, out cantidad);

            if (!VentaNegocio.ValidarCantidad(cantidad, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = new ItemVenta
            {
                IdProducto = 0, // Sin conexión a BD: se completa al integrar el catálogo real
                Nombre = nombre,
                PrecioVenta = precioUnitario,
                Cantidad = cantidad
            };

            _detalleVenta.Add(item);

            // Limpiar campos de carga rápida
            txtProductoBusqueda.Clear();
            txtPrecioUnitario.Clear();
            txtCantidad.Text = "1";

            ActualizarTotal();
        }

        private void BtnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton == null) return;

            var item = boton.Tag as ItemVenta;
            if (item == null) return;

            _detalleVenta.Remove(item);
            ActualizarTotal();
        }

        // ==========================================================
        // Refresco de Total y Vuelto (el cálculo real vive en VentaNegocio)
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

        /// <summary>
        /// Recalcula el vuelto cada vez que cambia el tipo de pago.
        /// El campo "Monto Recibido" queda siempre editable, sin importar
        /// el método elegido.
        /// </summary>
        private void ActualizarEstadoPago()
        {
            if (cmbTipoPago == null || txtMontoRecibido == null || txtVuelto == null) return;

            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);

            if (string.IsNullOrWhiteSpace(txtMontoRecibido.Text))
            {
                txtMontoRecibido.Text = total.ToString("N2", CultureInfo.InvariantCulture);
            }

            CalcularVuelto();
        }

        private void CalcularVuelto()
        {
            if (txtMontoRecibido == null || txtVuelto == null) return;

            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);

            decimal recibido;
            decimal.TryParse(txtMontoRecibido.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out recibido);

            decimal vuelto = VentaNegocio.CalcularVuelto(total, recibido);

            txtVuelto.Text = vuelto.ToString("C2", CultureInfo.GetCultureInfo("es-AR"));
            txtVuelto.Foreground = vuelto < 0
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.Black;
        }

        // ==========================================================
        // Guardar / Cancelar (la validación real vive en VentaNegocio)
        // ==========================================================

        private void BtnGuardarVenta_Click(object sender, RoutedEventArgs e)
        {
            decimal total = VentaNegocio.CalcularTotal(_detalleVenta);

            decimal recibido;
            decimal.TryParse(txtMontoRecibido.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out recibido);

            if (!VentaNegocio.ValidarVenta(_detalleVenta.Count, total, recibido, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "No se puede guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Armar el detalle en texto con los productos agregados a la grilla
            // Ejemplo: "Reflector LED x2, Cámara de Seguridad x1"
            var listaNombres = new System.Collections.Generic.List<string>();
            foreach (var item in _detalleVenta)
            {
                listaNombres.Add($"{item.Nombre} x{item.Cantidad}");
            }
            string detalleProductos = string.Join(", ", listaNombres);

            // 2. Tomar el nombre del cliente (o asignarle Consumidor Final si está vacío)
            string nombreCliente = string.IsNullOrWhiteSpace(txtCliente.Text)
                ? "Consumidor Final"
                : txtCliente.Text.Trim();

            // 3. Tomar el método de pago seleccionado en el ComboBox
            string metodoPago = cmbTipoPago.SelectedItem is ComboBoxItem itemCombo
                ? itemCombo.Content.ToString()
                : cmbTipoPago.Text;

            // 4. REGISTRAR EN EL PADRÓN DE CLIENTES (DatosGlobales)
            DatosGlobales.RegistrarCompraCliente(
                nombre: nombreCliente,
                dni: "", // Si no tenés campo DNI en Ventas se guarda sin DNI
                telefono: "",
                detalle: detalleProductos,
                total: total,
                metodoPago: metodoPago
            );

            MessageBox.Show("¡Venta registrada con éxito y asociada al cliente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpiar el formulario para la siguiente venta
            BtnCancelarVenta_Click(sender, e);
        }

        private void BtnCancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            _detalleVenta.Clear();
            txtCliente.Clear();
            txtProductoBusqueda.Clear();
            txtPrecioUnitario.Clear();
            txtCantidad.Text = "1";
            cmbTipoPago.SelectedIndex = 0;
            ActualizarTotal();
        }
    }
}
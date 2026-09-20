using System;
using System.Collections.Generic;
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
    public partial class Productos : Page
    {
        private List<ProductoRow> _productosEstaticos => DatosGlobales.Productos;

        public Productos()
        {
            InitializeComponent();
            this.Loaded += Productos_Loaded;
            CargarCategorias();
        }

        private void Productos_Loaded(object sender, RoutedEventArgs e)
        {
            AplicarPermisosPorRol();
            CargarProductos();
        }

        private string ObtenerRolActual()
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow != null)
            {
                var propiedadRol = mainWindow.GetType().GetProperty("RolActual") ??
                                   mainWindow.GetType().GetProperty("RolUsuario");
                if (propiedadRol != null)
                {
                    return propiedadRol.GetValue(mainWindow)?.ToString() ?? string.Empty;
                }
            }

            return "Vendedor";
        }

        private void AplicarPermisosPorRol()
        {
            string rolActual = ObtenerRolActual();
            bool tienePermiso = ProductoNegocio.PuedeAdministrarCatalogo(rolActual);

            if (btnNuevoProducto != null)
            {
                btnNuevoProducto.Visibility = tienePermiso ? Visibility.Visible : Visibility.Collapsed;
            }

            if (columnaAcciones != null)
            {
                columnaAcciones.Visibility = tienePermiso ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CargarCategorias()
        {
            var categorias = new List<CategoriaItem>
            {
                new CategoriaItem { IdCategoria = 0, Nombre = "Todas" },
                new CategoriaItem { IdCategoria = 1, Nombre = "Herramientas" },
                new CategoriaItem { IdCategoria = 2, Nombre = "Hogar" },
                new CategoriaItem { IdCategoria = 3, Nombre = "Limpieza" },
                new CategoriaItem { IdCategoria = 4, Nombre = "Tecnología" }
            };

            cmbFiltroCategoria.ItemsSource = categorias;
            cmbFiltroCategoria.SelectedIndex = 0;
        }

        private void CargarProductos()
        {
            dgProductos.ItemsSource = null;
            dgProductos.ItemsSource = _productosEstaticos;
        }

        private void TipoProducto_Checked(object sender, RoutedEventArgs e)
        {
            if (panelHogar == null || panelTecnologia == null) return;

            bool esHogar = rbHogar.IsChecked == true;
            panelHogar.Visibility = esHogar ? Visibility.Visible : Visibility.Collapsed;
            panelTecnologia.Visibility = esHogar ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string termino = txtBusqueda.Text?.Trim().ToLower() ?? string.Empty;
            var catSeleccionada = cmbFiltroCategoria.SelectedItem as CategoriaItem;

            var filtrados = _productosEstaticos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                filtrados = filtrados.Where(p => p.NombreProducto.ToLower().Contains(termino) ||
                                                 p.IdProducto.ToString().Contains(termino));
            }

            if (catSeleccionada != null && catSeleccionada.IdCategoria > 0)
            {
                filtrados = filtrados.Where(p => string.Equals(p.NombreCategoria, catSeleccionada.Nombre, StringComparison.OrdinalIgnoreCase));
            }

            dgProductos.ItemsSource = null;
            dgProductos.ItemsSource = filtrados.ToList();
        }

        private void DgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // ==========================================================
        // MANEJO DE LA VENTANA MODAL (ALTA Y EDICIÓN)
        // ==========================================================

        private void BtnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            if (!ProductoNegocio.ValidarGuardado(ObtenerRolActual(), out string errorRol))
            {
                MessageBox.Show(errorRol, "Acceso restringido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LimpiarFormulario();
            ModalFormulario.Visibility = Visibility.Visible;
            txtNombre.Focus();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (!ProductoNegocio.ValidarGuardado(ObtenerRolActual(), out string errorRol))
            {
                MessageBox.Show(errorRol, "Acceso restringido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var boton = sender as Button;
            if (boton == null || !(boton.Tag is int idProducto)) return;

            var producto = _productosEstaticos.FirstOrDefault(p => p.IdProducto == idProducto);
            if (producto == null) return;

            txtId.Text = producto.IdProducto.ToString();
            txtNombre.Text = producto.NombreProducto;
            txtPrecioCosto.Text = producto.PrecioCosto.ToString("0.00", CultureInfo.InvariantCulture);
            txtPrecioVenta.Text = producto.PrecioVentas.ToString("0.00", CultureInfo.InvariantCulture);
            txtStock.Text = producto.Stock.ToString();

            if (string.Equals(producto.TipoProducto, "Hogar", StringComparison.OrdinalIgnoreCase))
            {
                rbHogar.IsChecked = true;
            }
            else
            {
                rbTecnologia.IsChecked = true;
            }

            ModalFormulario.Visibility = Visibility.Visible;
            txtNombre.Focus();
        }

        // Reemplaza al antiguo BtnEliminar_Click: ahora hace baja/reactivación lógica en vez de borrar
        private void BtnToggleActivo_Click(object sender, RoutedEventArgs e)
        {
            if (!ProductoNegocio.ValidarEliminacion(ObtenerRolActual(), out string errorRol))
            {
                MessageBox.Show(errorRol, "Acceso restringido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(sender is Button boton) || !(boton.Tag is ProductoRow producto)) return;

            bool vaAActivar = !producto.Activo;
            string accion = vaAActivar ? "reactivar" : "dar de baja a";

            var resultado = MessageBox.Show(
                $"¿Está seguro que desea {accion} el producto \"{producto.NombreProducto}\"?",
                vaAActivar ? "Confirmar reactivación" : "Confirmar baja de producto",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                producto.Activo = vaAActivar;
            }
        }

        // ==========================================================
        // VALIDACIÓN Y GUARDADO DE FORMULARIO
        // ==========================================================

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ProductoNegocio.ValidarGuardado(ObtenerRolActual(), out string errorRol))
            {
                MessageBox.Show(errorRol, "Acceso restringido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool esHogar = rbHogar.IsChecked == true;

            if (!ProductoNegocio.ValidarIngresoStock(
                txtNombre.Text?.Trim(),
                txtPrecioCosto.Text?.Trim(),
                txtPrecioVenta.Text?.Trim(),
                txtStock.Text?.Trim(),
                esHogar,
                txtMaterial.Text?.Trim(),
                txtAmbiente.Text?.Trim(),
                txtMarca.Text?.Trim(),
                txtModelo.Text?.Trim(),
                txtGarantia.Text?.Trim(),
                out string errorValidacion))
            {
                MessageBox.Show(errorValidacion, "Validación de Formulario", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal pCosto = decimal.Parse(txtPrecioCosto.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            decimal pVenta = decimal.Parse(txtPrecioVenta.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
            int stock = int.Parse(txtStock.Text.Trim());
            string tipoProd = esHogar ? "Hogar" : "Tecnología";

            if (string.IsNullOrEmpty(txtId.Text))
            {
                int nuevoId = _productosEstaticos.Count > 0 ? _productosEstaticos.Max(p => p.IdProducto) + 1 : 1;

                _productosEstaticos.Add(new ProductoRow
                {
                    IdProducto = nuevoId,
                    NombreProducto = txtNombre.Text.Trim(),
                    PrecioCosto = pCosto,
                    PrecioVentas = pVenta,
                    Stock = stock,
                    TipoProducto = tipoProd,
                    NombreCategoria = tipoProd
                });

                MessageBox.Show("¡Producto agregado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                int idEditado = int.Parse(txtId.Text);
                var productoAEditar = _productosEstaticos.FirstOrDefault(p => p.IdProducto == idEditado);

                if (productoAEditar != null)
                {
                    productoAEditar.NombreProducto = txtNombre.Text.Trim();
                    productoAEditar.PrecioCosto = pCosto;
                    productoAEditar.PrecioVentas = pVenta;
                    productoAEditar.Stock = stock;
                    productoAEditar.TipoProducto = tipoProd;

                    MessageBox.Show("¡Producto modificado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            LimpiarFormulario();
            CargarProductos();
            ModalFormulario.Visibility = Visibility.Collapsed;
        }

        // ==========================================================
        // FILTROS DE TECLADO
        // ==========================================================

        public void ValidarSoloEnteros(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public void ValidarSoloDecimales(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string textoNuevo = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            Regex regex = new Regex(@"^\d*([.,]\d{0,2})?$");
            e.Handled = !regex.IsMatch(textoNuevo);
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            ModalFormulario.Visibility = Visibility.Collapsed;
        }

        private void LimpiarFormulario()
        {
            txtId.Clear();
            txtNombre.Clear();
            txtPrecioCosto.Clear();
            txtPrecioVenta.Clear();
            txtStock.Clear();

            txtMaterial.Clear();
            txtDimensiones.Clear();
            txtPeso.Clear();
            txtAmbiente.Clear();

            txtMarca.Clear();
            txtModelo.Clear();
            txtGarantia.Clear();
            txtVoltaje.Clear();

            rbHogar.IsChecked = true;
            dgProductos.SelectedItem = null;
        }
    }
}
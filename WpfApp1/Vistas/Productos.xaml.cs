using System;
using System.Collections.Generic;
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
        // 1. Lista estática que simulará nuestra Base de Datos en memoria
        private List<ProductoRow> _productosEstaticos => DatosGlobales.Productos;
        public Productos()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
        }

        private void CargarCategorias()
        {
            var categorias = new List<CategoriaItem>
            {
                new CategoriaItem { IdCategoria = 0, Nombre = "Todas" },
                new CategoriaItem { IdCategoria = 1, Nombre = "Herramientas" },
                new CategoriaItem { IdCategoria = 2, Nombre = "Textiles" },
                new CategoriaItem { IdCategoria = 3, Nombre = "Electrónica" },
            };

            cmbFiltroCategoria.ItemsSource = categorias;
            cmbFiltroCategoria.SelectedIndex = 0;
        }

        private void CargarProductos()
        {
            dgProductos.ItemsSource = null;
            dgProductos.ItemsSource = _productosEstaticos;
            ActualizarResumenInventario();
        }

        private void ActualizarResumenInventario()
        {
            // Estos campos se eliminaron del XAML para tener un diseño más limpio (Soluciona el error CS0103)
            // txtNumeroProductos.Text = "0";
            // txtPrecioTotalInventario.Text = "0.00";
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
            var termino = txtBusqueda.Text?.Trim();
            var categoriaSeleccionada = cmbFiltroCategoria.SelectedValue as int?;
        }

        private void DgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var producto = dgProductos.SelectedItem as ProductoRow;
            if (producto == null) return;

            txtId.Text = producto.IdProducto.ToString();
            txtNombre.Text = producto.NombreProducto;
            txtPrecioVenta.Text = producto.PrecioVentas.ToString("N2");
            txtStock.Text = producto.Stock.ToString();

            if (string.Equals(producto.TipoProducto, "Hogar", StringComparison.OrdinalIgnoreCase))
            {
                rbHogar.IsChecked = true;
            }
            else
            {
                rbTecnologia.IsChecked = true;
            }
        }

        // ==========================================================
        // MANEJO DE LA VENTANA MODAL (ALTA Y EDICIÓN)
        // ==========================================================

        private void BtnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            // Muestra el panel modal oscuro por encima
            ModalFormulario.Visibility = Visibility.Visible;
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton == null || !(boton.Tag is int idProducto)) return;

            // 4. Buscar el producto en la lista estática
            var producto = _productosEstaticos.Find(p => p.IdProducto == idProducto);
            if (producto == null) return;

            // 5. Cargar los datos en el modal
            txtId.Text = producto.IdProducto.ToString();
            txtNombre.Text = producto.NombreProducto;
            txtPrecioCosto.Text = producto.PrecioCosto.ToString("0.00");
            txtPrecioVenta.Text = producto.PrecioVentas.ToString("0.00");
            txtStock.Text = producto.Stock.ToString();

            if (string.Equals(producto.TipoProducto, "Hogar", StringComparison.OrdinalIgnoreCase))
            {
                rbHogar.IsChecked = true;
            }
            else
            {
                rbTecnologia.IsChecked = true;
            }

            // Muestra el panel modal oscuro por encima
            ModalFormulario.Visibility = Visibility.Visible;
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton == null || !(boton.Tag is int idProducto)) return;

            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar este producto?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                // 6. Eliminar el producto de la lista estática
                var productoAEliminar = _productosEstaticos.Find(p => p.IdProducto == idProducto);
                if (productoAEliminar != null)
                {
                    _productosEstaticos.Remove(productoAEliminar);
                    CargarProductos(); // Refresca la tabla
                    MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // ==========================================================
        // VALIDACIÓN Y GUARDADO DE FORMULARIO
        // ==========================================================

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormularioCompleto(out string errorValidacion))
            {
                MessageBox.Show(errorValidacion, "Validación de Formulario", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 7. Simular guardado estático (Alta y Modificación)
            decimal pCosto = decimal.Parse(txtPrecioCosto.Text.Replace('.', ','));
            decimal pVenta = decimal.Parse(txtPrecioVenta.Text.Replace('.', ','));
            int stock = int.Parse(txtStock.Text);
            string tipoProd = rbHogar.IsChecked == true ? "Hogar" : "Tecnología";

            if (string.IsNullOrEmpty(txtId.Text))
            {
                // Es un ALTA (crear ID ficticio)
                int nuevoId = _productosEstaticos.Count > 0 ? _productosEstaticos[_productosEstaticos.Count - 1].IdProducto + 1 : 1;

                _productosEstaticos.Add(new ProductoRow
                {
                    IdProducto = nuevoId,
                    NombreProducto = txtNombre.Text,
                    PrecioCosto = pCosto,
                    PrecioVentas = pVenta,
                    Stock = stock,
                    TipoProducto = tipoProd,
                    NombreCategoria = "Sin Categoría" // Ficticio por ahora
                });

                MessageBox.Show("¡Producto agregado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Es una MODIFICACIÓN
                int idEditado = int.Parse(txtId.Text);
                var productoAEditar = _productosEstaticos.Find(p => p.IdProducto == idEditado);

                if (productoAEditar != null)
                {
                    productoAEditar.NombreProducto = txtNombre.Text;
                    productoAEditar.PrecioCosto = pCosto;
                    productoAEditar.PrecioVentas = pVenta;
                    productoAEditar.Stock = stock;
                    productoAEditar.TipoProducto = tipoProd;

                    MessageBox.Show("¡Producto modificado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            LimpiarFormulario();
            CargarProductos(); // Refresca la tabla
            ModalFormulario.Visibility = Visibility.Collapsed;
        }

        private bool ValidarFormularioCompleto(out string error)
        {
            // 1. Validaciones Generales
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                error = "El nombre del producto es obligatorio.";
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrecioCosto.Text) ||
                !decimal.TryParse(txtPrecioCosto.Text.Replace('.', ','), out decimal pCosto) || pCosto <= 0)
            {
                error = "Debe ingresar un precio de costo válido mayor a 0.";
                txtPrecioCosto.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPrecioVenta.Text) ||
                !decimal.TryParse(txtPrecioVenta.Text.Replace('.', ','), out decimal pVenta) || pVenta <= 0)
            {
                error = "Debe ingresar un precio de venta válido mayor a 0.";
                txtPrecioVenta.Focus();
                return false;
            }

            if (pVenta < pCosto)
            {
                error = "El precio de venta no puede ser inferior al precio de costo.";
                txtPrecioVenta.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtStock.Text) || !int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                error = "Debe ingresar una cantidad de stock válida (número entero >= 0).";
                txtStock.Focus();
                return false;
            }

            // 2. Validaciones Condicionales según el tipo de producto
            if (rbHogar.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(txtMaterial.Text))
                {
                    error = "Para productos de Hogar, el campo Material es obligatorio.";
                    txtMaterial.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtAmbiente.Text))
                {
                    error = "Para productos de Hogar, el campo Ambiente es obligatorio.";
                    txtAmbiente.Focus();
                    return false;
                }
            }
            else if (rbTecnologia.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(txtMarca.Text))
                {
                    error = "Para productos de Tecnología, la Marca es obligatoria.";
                    txtMarca.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtModelo.Text))
                {
                    error = "Para productos de Tecnología, el Modelo es obligatorio.";
                    txtModelo.Focus();
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(txtGarantia.Text) && !int.TryParse(txtGarantia.Text, out int garantiaMeses))
                {
                    error = "La garantía debe ser un número entero de meses.";
                    txtGarantia.Focus();
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        // ==========================================================
        // FILTROS EN TIEMPO REAL (PREVIEW TEXT INPUT)
        // ==========================================================

        public void ValidarSoloEnteros(object sender, TextCompositionEventArgs e)
        {
            // Solo dígitos 0 al 9
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public void ValidarSoloDecimales(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string textoNuevo = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Permite dígitos y un único separador (. o ,) con hasta 2 decimales
            Regex regex = new Regex(@"^\d*([.,]\d{0,2})?$");
            e.Handled = !regex.IsMatch(textoNuevo);
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            // Oculta el modal cuando se presiona Cancelar
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
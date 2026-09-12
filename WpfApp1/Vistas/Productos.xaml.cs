using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TiendaOga.Entidades;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    /// <summary>
    /// Página de gestión de productos (Panel Principal > Gestión de Productos).
    /// Estilo dashboard minimalista: tarjetas blancas sobre fondo gris claro,
    /// acento naranja, formulario condicional Hogar / Tecnología.
    /// </summary>
    public partial class Productos : Page
    {
        public Productos()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
        }

        // ==========================================================
        // Carga inicial (reemplazar con las llamadas reales a tu capa
        // de datos / servicio de productos y categorías)
        // ==========================================================

        private void CargarCategorias()
        {
            // TODO: reemplazar por la consulta real a la tabla `categoria`
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
            // TODO: reemplazar por la consulta real a la tabla `producto`
            // (con join a producto_hogar / producto_tecnologia según corresponda)
            dgProductos.ItemsSource = new List<ProductoRow>();

            ActualizarResumenInventario();
        }

        private void ActualizarResumenInventario()
        {
            // TODO: calcular a partir de dgProductos.ItemsSource
            txtNumeroProductos.Text = "0";
            txtPrecioTotalInventario.Text = "0.00";
        }

        // ==========================================================
        // Alternancia de paneles Hogar / Tecnología
        // ==========================================================

        private void TipoProducto_Checked(object sender, RoutedEventArgs e)
        {
            // panelHogar / panelTecnologia pueden no existir aún si el evento
            // se dispara durante InitializeComponent()
            if (panelHogar == null || panelTecnologia == null) return;

            bool esHogar = rbHogar.IsChecked == true;
            panelHogar.Visibility = esHogar ? Visibility.Visible : Visibility.Collapsed;
            panelTecnologia.Visibility = esHogar ? Visibility.Collapsed : Visibility.Visible;
        }

        // ==========================================================
        // Búsqueda y filtro
        // ==========================================================

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var termino = txtBusqueda.Text?.Trim();
            var categoriaSeleccionada = cmbFiltroCategoria.SelectedValue as int?;

            // TODO: aplicar filtro real por id_categoria y por ID/Nombre
            // dgProductos.ItemsSource = servicioProductos.Buscar(termino, categoriaSeleccionada);
        }

        // ==========================================================
        // Selección de fila en la grilla -> cargar formulario
        // ==========================================================

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
                // TODO: cargar txtMaterial, txtDimensiones, txtPeso, txtAmbiente
                // desde producto_hogar
            }
            else
            {
                rbTecnologia.IsChecked = true;
                // TODO: cargar txtMarca, txtModelo, txtGarantia, txtVoltaje
                // desde producto_tecnologia
            }
        }

        // ==========================================================
        // Acciones de fila: Editar / Eliminar
        // ==========================================================

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton == null || !(boton.Tag is int idProducto)) return;
            // TODO: cargar el producto por idProducto en el formulario inferior
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
                // TODO: eliminar producto por idProducto y recargar la grilla
                CargarProductos();
            }
        }

        // ==========================================================
        // Guardar / Cancelar formulario
        // ==========================================================

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ProductoNegocio.ValidarProducto(txtNombre.Text, txtPrecioVenta.Text, txtStock.Text, out string mensajeError))
            {
                MessageBox.Show(mensajeError, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: mapear los campos del formulario a la entidad Producto
            // (+ ProductoHogar o ProductoTecnologia según rbHogar/rbTecnologia)
            // y persistir vía el servicio correspondiente.

            LimpiarFormulario();
            CargarProductos();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
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
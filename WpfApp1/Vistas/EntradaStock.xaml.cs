using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class EntradaStock : Page
    {
        public class ItemIngresoStock
        {
            public int Codigo { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
            public string Tipo { get; set; }
            public decimal PrecioCosto { get; set; }
            public decimal PrecioVenta { get; set; }
            public int Stock { get; set; }
            public string FechaIngreso { get; set; }
        }

        private static ObservableCollection<ItemIngresoStock> listaIngresos = new ObservableCollection<ItemIngresoStock>();
        private static int contadorCodigo = 101;

        public EntradaStock()
        {
            InitializeComponent();
            CargarCategorias();
            dgIngresos.ItemsSource = listaIngresos;
        }

        private void CargarCategorias()
        {
            cmbCategoria.ItemsSource = new List<string>
            {
                "Herramientas",
                "Electrónica",
                "Bazar y Hogar",
                "Iluminación",
                "Cuidado Personal"
            };
            cmbCategoria.SelectedIndex = 0;
        }

        private void TipoProducto_Checked(object sender, RoutedEventArgs e)
        {
            if (panelHogar == null || panelTecnologia == null) return;

            bool esHogar = rbHogar.IsChecked == true;
            panelHogar.Visibility = esHogar ? Visibility.Visible : Visibility.Collapsed;
            panelTecnologia.Visibility = esHogar ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            bool esValido = ProductoNegocio.ValidarIngresoStock(
                txtNombre.Text,
                cmbCategoria.SelectedIndex,
                txtPrecioCosto.Text,
                txtPrecioVenta.Text,
                txtStock.Text,
                rbHogar.IsChecked == true,
                txtMaterial.Text,
                txtAmbiente.Text,
                txtMarca.Text,
                txtModelo.Text,
                out string mensajeError
            );

            if (!esValido)
            {
                MessageBox.Show(mensajeError, "Validación requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal costo = decimal.Parse(txtPrecioCosto.Text.Replace('.', ','));
            decimal venta = decimal.Parse(txtPrecioVenta.Text.Replace('.', ','));
            int stock = int.Parse(txtStock.Text);
            string tipo = rbHogar.IsChecked == true ? "Hogar" : "Tecnología";

            listaIngresos.Insert(0, new ItemIngresoStock
            {
                Codigo = contadorCodigo++,
                Nombre = txtNombre.Text.Trim(),
                Categoria = cmbCategoria.SelectedItem.ToString(),
                Tipo = tipo,
                PrecioCosto = costo,
                PrecioVenta = venta,
                Stock = stock,
                FechaIngreso = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });

            MessageBox.Show("Stock registrado exitosamente en el sistema.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarFormulario();
        }

        public void ValidarSoloEnteros(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public void ValidarSoloDecimales(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string nuevoTexto = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            Regex regex = new Regex(@"^\d*([.,]\d{0,2})?$");
            e.Handled = !regex.IsMatch(nuevoTexto);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
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
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;
            txtNombre.Focus();
        }
    }
}
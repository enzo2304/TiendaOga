using System.Windows;

namespace TiendaOga
{
    public partial class Window1 : Window
    {
        private string usuarioActual;
        private string rolActual;

        public Window1(string usuario, string rol)
        {
            InitializeComponent();
            usuarioActual = usuario;
            rolActual = rol;

            ConfigurarVistaPorRol();
        }

        public Window1() : this("admin", "Administrador") { }

        private void ConfigurarVistaPorRol()
        {
            lblUsuarioActivo.Text = $"Usuario: {usuarioActual}";
            lblRolActivo.Text = $"Rol: {rolActual}";

            // Restricciones de acceso según perfil
            if (rolActual == "Vendedor")
            {
                lblHeaderAdmin.Visibility = Visibility.Collapsed;
                btnProductos.Visibility = Visibility.Collapsed;
                btnInventario.Visibility = Visibility.Collapsed;
                btnEntidades.Visibility = Visibility.Collapsed;
                btnUsuarios.Visibility = Visibility.Collapsed;

                lblHeaderGerente.Visibility = Visibility.Collapsed;
                btnReportesVentas.Visibility = Visibility.Collapsed;
                btnStockCritico.Visibility = Visibility.Collapsed;
            }
            else if (rolActual == "Gerente")
            {
                btnUsuarios.Visibility = Visibility.Collapsed;
                btnInventario.Visibility = Visibility.Collapsed;
            }
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        // ==========================================
        // ACÁ ESTÁ EL CAMBIO PRINCIPAL
        // ==========================================
        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // 1. Actualizamos el título de la barra superior
            lblTituloModulo.Text = "Módulo de Ventas y Facturación";

            // 2. Cargamos la página de ventas adentro del Frame
            ContenedorPrincipal.Navigate(new Ventas());
        }

        // Para los próximos módulos, la lógica será idéntica a btnVentas_Click
        private void btnConsultarStock_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Consulta de Catálogo y Stock";
            // FramePrincipal.Navigate(new PaginaCatalogo());
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Administración de Productos y Rubros";
            // FramePrincipal.Navigate(new PaginaProductos());
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Registro de Entrada y Ajuste de Stock";
        }

        private void btnEntidades_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Padrón de Clientes y Proveedores";
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Gestión de Cuentas de Usuario y Roles";
        }

        private void btnReportesVentas_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Reportes y Rendimiento de Ventas";
        }

        private void btnStockCritico_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Rotación de Productos y Stock Crítico";
        }
    }
}
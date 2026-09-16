using System.Windows;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
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

            // La decisión de qué puede ver cada rol vive en PermisosNegocio,
            // acá solo se aplica el resultado a los controles.
            btnProductos.Visibility = ToVisibility(PermisosNegocio.PuedeVerProductos(rolActual));
            btnEntradaStock.Visibility = ToVisibility(PermisosNegocio.PuedeVerEntradaStock(rolActual));
            btnClientes.Visibility = ToVisibility(PermisosNegocio.PuedeVerClientes(rolActual));
            btnUsuarios.Visibility = ToVisibility(PermisosNegocio.PuedeVerUsuarios(rolActual));
            btnReportesVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesGerencia(rolActual));
            btnStockCritico.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesGerencia(rolActual));
        }

        private static Visibility ToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
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
            // ContenedorPrincipal.Navigate(new PaginaCatalogo());
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Administración de Productos y Rubros";
            ContenedorPrincipal.Navigate(new Productos());
        }

        private void btnEntradaStock_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Registro de Entrada y Ajuste de Stock";
            ContenedorPrincipal.Navigate(new EntradaStock());
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Padrón de Clientes";
            ContenedorPrincipal.Navigate(new Clientes());
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Gestión de Cuentas de Usuario y Roles";
            ContenedorPrincipal.Navigate(new Usuarios());
        }
        private void btnReportesVentas_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Reportes y Rendimiento de Ventas";
            ContenedorPrincipal.Navigate(new ReportesVendedor());
        }
        

        private void btnStockCritico_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Rotación de Productos y Stock Crítico";
        }
    }
}
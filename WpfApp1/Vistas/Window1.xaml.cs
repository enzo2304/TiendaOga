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
            // btnEntradaStock.Visibility = ToVisibility(PermisosNegocio.PuedeVerEntradaStock(rolActual));
            btnClientes.Visibility = ToVisibility(PermisosNegocio.PuedeVerClientes(rolActual));
            btnUsuarios.Visibility = ToVisibility(PermisosNegocio.PuedeVerUsuarios(rolActual));
            btnReportesVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesGerencia(rolActual));

            // OJO: el XAML actual ya no tiene "btnStockCritico" (ese botón se
            // reemplazó por "btnReporteGeneral" / Panel Gerencial), así que el
            // permiso de gerencia ahora se aplica sobre ese botón.
            btnReporteGeneral.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesGerencia(rolActual));
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

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Módulo de Ventas y Facturación";
            ContenedorPrincipal.Navigate(new Ventas());
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Administración de Productos y Rubros";
            ContenedorPrincipal.Navigate(new Productos());
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

        // ==========================================
        // CONEXIÓN CON EL PANEL GERENCIAL
        // ==========================================
        private void btnReporteGeneral_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Panel Gerencial";
            ContenedorPrincipal.Navigate(new ReporteGeneral());
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Backup del Sistema";
            // TODO: cuando tengas la vista de Backup armada, navegala igual que las demás:
            // ContenedorPrincipal.Navigate(new Backup());
        }
    }
}
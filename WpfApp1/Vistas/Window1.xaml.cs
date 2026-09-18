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
            btnVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerVentas(rolActual));
            btnClientes.Visibility = ToVisibility(PermisosNegocio.PuedeVerClientes(rolActual));
            btnProductos.Visibility = ToVisibility(PermisosNegocio.PuedeVerProductos(rolActual));
            btnUsuarios.Visibility = ToVisibility(PermisosNegocio.PuedeVerUsuarios(rolActual));
            btnReportesVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesVendedor(rolActual));
            btnReporteGeneral.Visibility = ToVisibility(PermisosNegocio.PuedeVerReporteGeneral(rolActual));
            btnStockBackup.Visibility = ToVisibility(PermisosNegocio.PuedeVerBackup(rolActual));
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
            if (!PermisosNegocio.PuedeVerVentas(rolActual)) return;
            lblTituloModulo.Text = "Módulo de Ventas y Facturación";
            ContenedorPrincipal.Navigate(new Ventas());
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerProductos(rolActual)) return;
            lblTituloModulo.Text = "Administración de Productos y Rubros";
            ContenedorPrincipal.Navigate(new Productos());
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerClientes(rolActual)) return;
            lblTituloModulo.Text = "Padrón de Clientes";
            ContenedorPrincipal.Navigate(new Clientes());
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerUsuarios(rolActual)) return;
            lblTituloModulo.Text = "Gestión de Cuentas de Usuario y Roles";
            ContenedorPrincipal.Navigate(new Usuarios());
        }

        private void btnReportesVentas_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerReportesVendedor(rolActual)) return;
            lblTituloModulo.Text = "Reportes y Rendimiento de Ventas";
            ContenedorPrincipal.Navigate(new ReportesVendedor());
        }

        // ==========================================
        // CONEXIÓN CON EL PANEL GERENCIAL
        // ==========================================
        private void btnReporteGeneral_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerReporteGeneral(rolActual)) return;
            lblTituloModulo.Text = "Panel Gerencial";
            ContenedorPrincipal.Navigate(new ReporteGeneral());
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerBackup(rolActual)) return;
            lblTituloModulo.Text = "Backup del Sistema";
            ContenedorPrincipal.Navigate(new Backup());
        }
    }
}
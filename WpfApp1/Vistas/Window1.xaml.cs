using System;
using System.Windows;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Window1 : Window
    {
        public string UsuarioActual { get; private set; }
        public string RolActual { get; private set; }

        public Window1(string usuario, string rol)
        {
            InitializeComponent();
            UsuarioActual = usuario;
            RolActual = rol;

            ConfigurarVistaPorRol();
        }

        public Window1() : this("admin", "Administrador") { }

        private void ConfigurarVistaPorRol()
        {
            lblUsuarioActivo.Text = $"Usuario: {UsuarioActual}";
            lblRolActivo.Text = $"Rol: {RolActual}";

            btnVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerVentas(RolActual));
            btnClientes.Visibility = ToVisibility(PermisosNegocio.PuedeVerClientes(RolActual));
            btnProductos.Visibility = ToVisibility(PermisosNegocio.PuedeVerProductos(RolActual));
            btnUsuarios.Visibility = ToVisibility(PermisosNegocio.PuedeVerUsuarios(RolActual));
            btnReportesVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesVendedor(RolActual));
            btnReporteGeneral.Visibility = ToVisibility(PermisosNegocio.PuedeVerReporteGeneral(RolActual));
            btnStockBackup.Visibility = ToVisibility(PermisosNegocio.PuedeVerBackup(RolActual));

            // El texto del botón cambia según el alcance que le corresponde al rol,
            // pero ambos casos navegan igual (btnReportesVentas_Click no cambia)
            TipoReporteVenta alcanceBoton = PermisosNegocio.ObtenerAlcanceReporteVenta(RolActual);
            btnReportesVentas.Content = alcanceBoton == TipoReporteVenta.Individual
                ? "Cierre de Caja"
                : "Reporte Vendedor";

            if (PermisosNegocio.PuedeVerVentas(RolActual))
            {
                btnVentas_Click(null, null);
            }
            else if (PermisosNegocio.PuedeVerUsuarios(RolActual))
            {
                btnUsuarios_Click(null, null);
            }
            else if (PermisosNegocio.PuedeVerReporteGeneral(RolActual))
            {
                btnReporteGeneral_Click(null, null);
            }
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
            if (!PermisosNegocio.PuedeVerVentas(RolActual)) return;
            lblTituloModulo.Text = "Módulo de Ventas y Facturación";
            ContenedorPrincipal.Navigate(new Ventas());
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerProductos(RolActual)) return;
            lblTituloModulo.Text = "Administración de Productos y Rubros";
            ContenedorPrincipal.Navigate(new Productos());
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerClientes(RolActual)) return;
            lblTituloModulo.Text = "Padrón de Clientes";
            ContenedorPrincipal.Navigate(new Clientes());
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerUsuarios(RolActual)) return;
            lblTituloModulo.Text = "Gestión de Cuentas de Usuario y Roles";
            ContenedorPrincipal.Navigate(new Usuarios());
        }

        private void btnReportesVentas_Click(object sender, RoutedEventArgs e)
        {
            TipoReporteVenta alcance = PermisosNegocio.ObtenerAlcanceReporteVenta(RolActual);

            switch (alcance)
            {
                case TipoReporteVenta.Consolidado:
                    lblTituloModulo.Text = "Reporte de Ventas por Vendedor (Consolidado)";
                    ContenedorPrincipal.Navigate(new ReportesVendedor());
                    break;

                case TipoReporteVenta.Individual:
                    lblTituloModulo.Text = "Cierre de Caja y Reportes del Vendedor";
                    ContenedorPrincipal.Navigate(new ReporteVendedorIndividual());
                    break;

                default:
                    MessageBox.Show("No cuenta con permisos para consultar reportes de venta.",
                                    "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        private void btnReporteGeneral_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerReporteGeneral(RolActual)) return;
            lblTituloModulo.Text = "Panel Gerencial";
            ContenedorPrincipal.Navigate(new ReporteGeneral());
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerBackup(RolActual)) return;
            lblTituloModulo.Text = "Backup del Sistema";
            ContenedorPrincipal.Navigate(new Backup());
        }
    }
}
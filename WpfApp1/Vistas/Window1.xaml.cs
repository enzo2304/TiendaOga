using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using TiendaOga.Negocio;

namespace TiendaOga.Vistas
{
    public partial class Window1 : Window
    {
        public string UsuarioActual { get; private set; }
        public string RolActual { get; private set; }
        public int IdUsuarioActual { get; private set; }

        private DateTime? _lineaBaseRestore;
        private DispatcherTimer _vigilante;
        public bool VigilanciaPausada { get; set; }

        public Window1(string usuario, string rol)
        {
            InitializeComponent();
            UsuarioActual = usuario;
            RolActual = rol;
            IdUsuarioActual = UsuarioNegocio.ObtenerIdPorLogin(usuario);
            ConfigurarVistaPorRol();

            IniciarVigilanciaRestore();
        }

        public Window1() : this("admin", "Administrador") { }

        // ------------------------------------------------------------
        // VIGILANCIA DE RESTAURACIONES
        // ------------------------------------------------------------
        public void ActualizarLineaBaseRestore()
        {
            DateTime? v;
            if (BackupNegocio.TryObtenerUltimoRestore(out v)) _lineaBaseRestore = v;
        }

        private void IniciarVigilanciaRestore()
        {
            ActualizarLineaBaseRestore();
            _vigilante = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _vigilante.Tick += (s, e) =>
            {
                if (VigilanciaPausada) return;

                DateTime? actual;
                if (!BackupNegocio.TryObtenerUltimoRestore(out actual)) return;
                if (actual == _lineaBaseRestore) return;

                _vigilante.Stop();
                SqlConnection.ClearAllPools();
                MessageBox.Show("El administrador restauró la base de datos.\nPor seguridad, tu sesión ha finalizado.",
                                "Sesión finalizada", MessageBoxButton.OK, MessageBoxImage.Information);
                btnCerrarSesion_Click(null, null);
            };
            _vigilante.Start();
        }

        // Se llama desde la pantalla de Backup, justo después de restaurar.
        // Devuelve true si la sesión sigue siendo válida; false si se cerró.
        // Compara por login (no por id): el login es lo estable entre respaldos.
        public bool RevalidarSesionTrasRestore()
        {
            int id = 0;
            string rol = null;

            try
            {
                UsuarioNegocio.ObtenerDatosSesion(UsuarioActual, out id, out rol);
            }
            catch
            {
                // Si no se puede verificar, se trata como inválida (id = 0)
            }

            bool existe = id > 0;
            bool mismoRol = existe && string.Equals((rol ?? string.Empty).Trim(),
                                                    (RolActual ?? string.Empty).Trim(),
                                                    StringComparison.OrdinalIgnoreCase);

            if (!existe || !mismoRol)
            {
                string motivo = !existe
                    ? "Tu cuenta no existe (o está inactiva) en el respaldo restaurado."
                    : "Tu rol cambió en el respaldo restaurado.";

                MessageBox.Show(motivo + "\nPor seguridad, se cerrará la sesión.",
                                "Sesión finalizada", MessageBoxButton.OK, MessageBoxImage.Warning);
                btnCerrarSesion_Click(null, null);
                return false;
            }

            // Mismo login y mismo rol, pero el id puede ser otro en el respaldo
            IdUsuarioActual = id;
            return true;
        }

        // ------------------------------------------------------------
        // CONFIGURACIÓN SEGÚN ROL
        // ------------------------------------------------------------
        private void ConfigurarVistaPorRol()
        {
            lblUsuarioActivo.Text = $"Usuario: {UsuarioActual}";
            lblRolActivo.Text = $"Rol: {RolActual}";

            // Gestión centralizada de visibilidad de los botones del menú
            btnVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerVentas(RolActual));
            btnClientes.Visibility = ToVisibility(PermisosNegocio.PuedeVerClientes(RolActual));
            btnProductos.Visibility = ToVisibility(PermisosNegocio.PuedeVerProductos(RolActual));
            btnUsuarios.Visibility = ToVisibility(PermisosNegocio.PuedeVerUsuarios(RolActual));
            btnReportesVentas.Visibility = ToVisibility(PermisosNegocio.PuedeVerReportesVendedor(RolActual));
            btnReporteProductos.Visibility = ToVisibility(PermisosNegocio.PuedeVerReporteProductos(RolActual));
            btnReporteGeneral.Visibility = ToVisibility(PermisosNegocio.PuedeVerReporteGeneral(RolActual));
            btnStockBackup.Visibility = ToVisibility(PermisosNegocio.PuedeVerBackup(RolActual));

            // Configuración dinámica del botón de ventas según el alcance
            if (PermisosNegocio.PuedeVerReportesVendedor(RolActual))
            {
                TipoReporteVenta alcanceBoton = PermisosNegocio.ObtenerAlcanceReporteVenta(RolActual);
                btnReportesVentas.Content = alcanceBoton == TipoReporteVenta.Individual
                    ? "Ventas del Día"
                    : "Reporte de ventas";
            }

            // Redirección automática inicial según permisos
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

        // ------------------------------------------------------------
        // NAVEGACIÓN
        // ------------------------------------------------------------
        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            _vigilante?.Stop();
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
                    lblTituloModulo.Text = "Resumen de Ventas Diarias";
                    ContenedorPrincipal.Navigate(new ReporteVendedorIndividual());
                    break;

                default:
                    MessageBox.Show("No cuenta con permisos para consultar reportes de venta.",
                                    "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        private void btnReporteProductos_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerReporteProductos(RolActual)) return;

            lblTituloModulo.Text = "Reporte de Productos Más Vendidos";
            ContenedorPrincipal.Navigate(new ReporteProductos());
        }

        private void btnReporteGeneral_Click(object sender, RoutedEventArgs e)
        {
            if (!PermisosNegocio.PuedeVerReporteGeneral(RolActual)) return;

            lblTituloModulo.Text = "Reporte General (Panel Gerencial)";
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
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
                // Ocultar sección de Administración
                lblHeaderAdmin.Visibility = Visibility.Collapsed;
                btnProductos.Visibility = Visibility.Collapsed;
                btnInventario.Visibility = Visibility.Collapsed;
                btnEntidades.Visibility = Visibility.Collapsed;
                btnUsuarios.Visibility = Visibility.Collapsed;

                // Ocultar sección de Gerencia
                lblHeaderGerente.Visibility = Visibility.Collapsed;
                btnReportesVentas.Visibility = Visibility.Collapsed;
                btnStockCritico.Visibility = Visibility.Collapsed;
            }
            else if (rolActual == "Gerente")
            {
                // El perfil Gerente no opera la gestión técnica de inventario ni cuentas
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

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Modulo de Ventas y Facturacion";
        }

        private void btnConsultarStock_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Consulta de Catalogo y Stock";
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Administracion de Productos y Rubros";
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Registro de Entrada y Ajuste de Stock";
        }

        private void btnEntidades_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Padron de Clientes y Proveedores";
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Gestion de Cuentas de Usuario y Roles";
        }

        private void btnReportesVentas_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Reportes y Rendimiento de Ventas";
        }

        private void btnStockCritico_Click(object sender, RoutedEventArgs e)
        {
            lblTituloModulo.Text = "Rotacion de Productos y Stock Critico";
        }
    }
}
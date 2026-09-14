namespace TiendaOga.Negocio
{
    /// <summary>
    /// Reglas de qué opciones del menú puede ver cada rol.
    /// Si mañana cambia qué ve un Vendedor o un Gerente, se cambia
    /// acá, no en el code-behind de la ventana.
    /// </summary>
    public static class PermisosNegocio
    {
        public static bool PuedeVerProductos(string rol)
        {
            return rol != "Vendedor";
        }

        public static bool PuedeVerEntradaStock(string rol)
        {
            return rol != "Vendedor" && rol != "Gerente";
        }

        public static bool PuedeVerClientes(string rol)
        {
            return rol != "Vendedor";
        }

        public static bool PuedeVerUsuarios(string rol)
        {
            return rol != "Vendedor" && rol != "Gerente";
        }

        public static bool PuedeVerReportesGerencia(string rol)
        {
            return rol != "Vendedor";
        }
    }
}

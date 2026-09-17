namespace TiendaOga
{
    /// <summary>
    /// Guarda los datos del usuario que inició sesión, accesibles
    /// desde cualquier parte de la app mientras dure la ejecución.
    /// </summary>
    public static class SesionActual
    {
        public static int IdUsuario { get; set; }
        public static string NombreCompleto { get; set; }
        public static string Rol { get; set; }
    }
}
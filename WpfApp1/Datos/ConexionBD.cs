using System.Data.SqlClient;

namespace TiendaOga.Datos
{
    public static class ConexionBD
    {
        // Conexión a tu instancia local de SQL Server con autenticación de Windows
        // Descomentar la línea según quién esté usando el proyecto:

        // Conexión de Enzo:
        private static string cadena = @"Server=(localdb)\ProjectModels;Database=TiendaOgaDB;Integrated Security=True;TrustServerCertificate=True;";

        // Conexión de Chris:
        // private static string cadena = @"Server=localhost\MSSQLSERVER01;Database=TiendaOgaDB;Integrated Security=True;TrustServerCertificate=True;";
        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadena);
        }
    }
}
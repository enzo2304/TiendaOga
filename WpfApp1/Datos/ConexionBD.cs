using System.Data.SqlClient;

namespace TiendaOga.Datos
{
    public static class ConexionBD
    {
        // Conexión a tu instancia local de SQL Server con autenticación de Windows
        private static string cadena = @"Server=localhost\MSSQLSERVER01;Database=TiendaOgaDB;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadena);
        }
    }
}
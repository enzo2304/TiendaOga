using Microsoft.Data.SqlClient;

namespace TiendaOga.Datos
{
    public class ConexionBD
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=TiendaOgaDB;Integrated Security=True;TrustServerCertificate=True;";

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(connectionString);
        }
    }
}
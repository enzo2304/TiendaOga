using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaOga.Datos
{
    public class PerfilRepository
    {
        // Instanciamos nuestra clase de conexión para obtener el "cable"
        private ConexionBD conexionBD = new ConexionBD();

        public bool EjecutarABMPerfil(char operacion, int? idPerfil = null, string nombrePerfil = null, string descripcion = null)
        {
            // Usamos la conexión que definimos en ConexionBD.cs
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                // Llamamos al Procedimiento Almacenado que creaste en SQL Server
                using (SqlCommand cmd = new SqlCommand("dbo.sp_ABM_Perfiles", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Parámetro @Operacion ('A' = Alta, 'B' = Baja, 'M' = Modificación)
                    cmd.Parameters.Add("@Operacion", SqlDbType.Char, 1).Value = operacion;

                    // 2. Parámetro @id_perfil (Maneja nulos por si es un Alta)
                    SqlParameter pId = new SqlParameter("@id_perfil", SqlDbType.Int);
                    pId.Value = idPerfil.HasValue ? (object)idPerfil.Value : DBNull.Value;
                    cmd.Parameters.Add(pId);

                    // 3. Parámetro @nombre_perfil
                    SqlParameter pNombre = new SqlParameter("@nombre_perfil", SqlDbType.VarChar, 50);
                    pNombre.Value = string.IsNullOrEmpty(nombrePerfil) ? (object)DBNull.Value : nombrePerfil;
                    cmd.Parameters.Add(pNombre);

                    // 4. Parámetro @descripcion
                    SqlParameter pDesc = new SqlParameter("@descripcion", SqlDbType.VarChar, 255);
                    pDesc.Value = string.IsNullOrEmpty(descripcion) ? (object)DBNull.Value : descripcion;
                    cmd.Parameters.Add(pDesc);

                    // Abrimos la conexión, ejecutamos el comando y verificamos si afectó filas
                    conn.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
    }
}
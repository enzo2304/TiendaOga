using System;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class BackupDatos
    {
        // La restauración se ejecuta desde master (no puede hacerse desde la base que se reemplaza)
        private SqlConnection ConexionMaster()
        {
            var modelo = new ConexionBD().ObtenerConexion();
            var cadena = new SqlConnectionStringBuilder(modelo.ConnectionString) { InitialCatalog = "master" };
            modelo.Dispose();
            return new SqlConnection(cadena.ConnectionString);
        }

        private static SqlCommand Comando(SqlConnection cn, string procedimiento)
        {
            return new SqlCommand(procedimiento, cn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 0
            };
        }

        public bool ProbarConexion()
        {
            try
            {
                using (var cn = new ConexionBD().ObtenerConexion())
                {
                    cn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Devuelve la ruta completa del archivo generado
        public string Crear(string carpeta, string nombreArchivo)
        {
            using (var cn = ConexionMaster())
            using (var cmd = Comando(cn, "dbo.usp_tienda_backup_crear"))
            {
                cmd.Parameters.AddWithValue("@ruta", carpeta);
                cmd.Parameters.AddWithValue("@nombre_archivo", nombreArchivo);
                cn.Open();
                return Convert.ToString(cmd.ExecuteScalar());
            }
        }

        public BackupInfo ObtenerInfo(string archivo)
        {
            using (var cn = ConexionMaster())
            using (var cmd = Comando(cn, "dbo.usp_tienda_backup_info"))
            {
                cmd.Parameters.AddWithValue("@archivo", archivo);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return new BackupInfo
                    {
                        NombreBase = rd["DatabaseName"].ToString(),
                        VersionMayor = Convert.ToInt32(rd["SoftwareVersionMajor"]),
                        NivelCompatibilidad = Convert.ToInt32(rd["CompatibilityLevel"]),
                        Tipo = Convert.ToInt32(rd["BackupType"]),
                        FechaBackup = Convert.ToDateTime(rd["BackupFinishDate"])
                    };
                }
            }
        }

        public int ObtenerVersionServidor()
        {
            using (var cn = ConexionMaster())
            using (var cmd = Comando(cn, "dbo.usp_tienda_servidor_info"))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    rd.Read();
                    return Convert.ToInt32(rd["version_mayor"]);
                }
            }
        }

        public double? ObtenerTamanioMb()
        {
            using (var cn = ConexionMaster())
            using (var cmd = Comando(cn, "dbo.usp_tienda_base_info"))
            {
                cn.Open();
                object valor = cmd.ExecuteScalar();
                return (valor == null || valor == DBNull.Value) ? (double?)null : Convert.ToDouble(valor);
            }
        }

        public int ContarConexiones()
        {
            using (var cn = ConexionMaster())
            using (var cmd = Comando(cn, "dbo.usp_tienda_conexiones_activas"))
            {
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Devuelve la ruta del backup preventivo (o null si no se hizo)
        public string Restaurar(string archivo, bool backupPrevio, string carpetaPrevio)
        {
            try
            {
                using (var cn = ConexionMaster())
                using (var cmd = Comando(cn, "dbo.usp_tienda_backup_restaurar"))
                {
                    cmd.Parameters.AddWithValue("@archivo", archivo);
                    cmd.Parameters.AddWithValue("@backup_previo", backupPrevio);
                    cmd.Parameters.AddWithValue("@carpeta_previo",
                        string.IsNullOrWhiteSpace(carpetaPrevio) ? (object)DBNull.Value : carpetaPrevio);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        int i = rd.GetOrdinal("backup_previo");
                        return rd.IsDBNull(i) ? null : rd.GetString(i);
                    }
                }
            }
            finally
            {
                // La restauración corta las conexiones: se descartan las que quedaron en el pool
                SqlConnection.ClearAllPools();
            }
        }
    }
}
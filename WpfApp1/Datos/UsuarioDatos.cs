using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    /// <summary>
    /// Acceso a datos de Usuario. Todo pasa por procedimientos
    /// almacenados (sp_ABM_Usuario, sp_Listar_Usuarios, etc.),
    /// nunca se arma SQL como texto acá.
    /// </summary>
    public class UsuarioDatos
    {
        private readonly ConexionBD conexionBD = new ConexionBD();

        public bool EjecutarABMUsuario(char operacion, int? idUsuario = null, int? idPerfil = null,
            string nombre = null, string apellido = null, string usuario = null,
            string password = null, string email = null)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                using (SqlCommand cmd = new SqlCommand("dbo.sp_ABM_Usuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Operacion", SqlDbType.Char, 1).Value = operacion;

                    SqlParameter pId = new SqlParameter("@id_usuario", SqlDbType.Int);
                    pId.Value = idUsuario.HasValue ? (object)idUsuario.Value : DBNull.Value;
                    cmd.Parameters.Add(pId);

                    SqlParameter pIdPerfil = new SqlParameter("@id_perfil", SqlDbType.Int);
                    pIdPerfil.Value = idPerfil.HasValue ? (object)idPerfil.Value : DBNull.Value;
                    cmd.Parameters.Add(pIdPerfil);

                    SqlParameter pNombre = new SqlParameter("@nombre", SqlDbType.VarChar, 100);
                    pNombre.Value = string.IsNullOrEmpty(nombre) ? (object)DBNull.Value : nombre;
                    cmd.Parameters.Add(pNombre);

                    SqlParameter pApellido = new SqlParameter("@apellido", SqlDbType.VarChar, 100);
                    pApellido.Value = string.IsNullOrEmpty(apellido) ? (object)DBNull.Value : apellido;
                    cmd.Parameters.Add(pApellido);

                    SqlParameter pUsuario = new SqlParameter("@usuario", SqlDbType.VarChar, 50);
                    pUsuario.Value = string.IsNullOrEmpty(usuario) ? (object)DBNull.Value : usuario;
                    cmd.Parameters.Add(pUsuario);

                    SqlParameter pPassword = new SqlParameter("@password", SqlDbType.VarChar, 255);
                    pPassword.Value = string.IsNullOrEmpty(password) ? (object)DBNull.Value : password;
                    cmd.Parameters.Add(pPassword);

                    SqlParameter pEmail = new SqlParameter("@email", SqlDbType.VarChar, 100);
                    pEmail.Value = string.IsNullOrEmpty(email) ? (object)DBNull.Value : email;
                    cmd.Parameters.Add(pEmail);

                    conn.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        public void GuardarUsuario(Usuario usuario)
        {
            EjecutarABMUsuario('A',
                idPerfil: usuario.id_perfil,
                nombre: usuario.nombre,
                apellido: usuario.apellido,
                usuario: usuario.usuario,
                password: usuario.password,
                email: usuario.email);
        }

        public List<PerfilItem> ObtenerPerfiles()
        {
            var perfiles = new List<PerfilItem>();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("dbo.sp_Listar_Perfiles_Activos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        perfiles.Add(new PerfilItem
                        {
                            IdPerfil = reader.GetInt32(reader.GetOrdinal("id_perfil")),
                            NombrePerfil = reader.GetString(reader.GetOrdinal("nombre_perfil"))
                        });
                    }
                }
            }

            return perfiles;
        }

        public List<UsuarioRow> ObtenerUsuarios()
        {
            var usuarios = new List<UsuarioRow>();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("dbo.sp_Listar_Usuarios", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new UsuarioRow
                        {
                            IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
                            IdPerfil = reader.GetInt32(reader.GetOrdinal("id_perfil")),
                            Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                            Apellido = reader.GetString(reader.GetOrdinal("apellido")),
                            UsuarioLogin = reader.GetString(reader.GetOrdinal("usuario")),
                            Email = reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString(reader.GetOrdinal("email")),
                            NombrePerfil = reader.GetString(reader.GetOrdinal("nombre_perfil")),
                            Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
                        });
                    }
                }
            }

            return usuarios;
        }

        public bool ExisteUsuario(string nombreUsuario)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("dbo.sp_Existe_Usuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@usuario", SqlDbType.VarChar, 50).Value = nombreUsuario;

                conn.Open();
                int cantidad = (int)cmd.ExecuteScalar();
                return cantidad > 0;
            }
        }

        public void ReactivarUsuario(int idUsuario)
        {
            EjecutarABMUsuario('R', idUsuario: idUsuario);
        }
    }
}
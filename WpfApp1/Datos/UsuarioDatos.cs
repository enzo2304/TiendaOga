using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    /// <summary>
    /// Acceso a datos de Usuario. Todo pasa por procedimientos

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

                    try
                    {
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        // Violación de UNIQUE KEY (email o usuario duplicado)

                        throw new InvalidOperationException(
                            "Ese email o nombre de usuario ya está en uso por otro usuario.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Da de alta un usuario nuevo.
        /// Devuelve null si se guardó correctamente, o un mensaje de error para mostrar al usuario.
        /// </summary>
        public string GuardarUsuario(Usuario usuario)
        {
            if (ExisteEmail(usuario.email))
            {
                return "Ese email ya está en uso por otro usuario.";
            }

            if (ExisteUsuario(usuario.usuario))
            {
                return "Ese nombre de usuario ya está en uso.";
            }

            try
            {
                EjecutarABMUsuario('A',
                    idPerfil: usuario.id_perfil,
                    nombre: usuario.nombre,
                    apellido: usuario.apellido,
                    usuario: usuario.usuario,
                    password: usuario.password,
                    email: usuario.email);

                return null; // null = todo OK
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Modifica un usuario existente.
        /// Devuelve null si se guardó correctamente, o un mensaje de error para mostrar al usuario.
        /// </summary>
        public string ModificarUsuario(Usuario usuario)
        {

            if (ExisteEmail(usuario.email, usuario.id_usuario))
            {
                return "Ese email ya está en uso por otro usuario.";
            }

            try
            {
                EjecutarABMUsuario('M',
                    idUsuario: usuario.id_usuario,
                    idPerfil: usuario.id_perfil,
                    nombre: usuario.nombre,
                    apellido: usuario.apellido,
                    usuario: usuario.usuario,
                    password: usuario.password,
                    email: usuario.email);

                return null; // null = todo OK
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
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

        public bool ExisteEmail(string email, int? idUsuarioExcluir = null)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("dbo.sp_Existe_Email", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = email;

                SqlParameter pIdExcluir = new SqlParameter("@id_usuario_excluir", SqlDbType.Int);
                pIdExcluir.Value = idUsuarioExcluir.HasValue ? (object)idUsuarioExcluir.Value : DBNull.Value;
                cmd.Parameters.Add(pIdExcluir);

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
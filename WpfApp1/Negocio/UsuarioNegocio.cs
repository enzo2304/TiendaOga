using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class UsuarioNegocio
    {
        private static readonly UsuarioDatos usuarioDatos = new UsuarioDatos();
        private const string PERFIL_ADMINISTRADOR = "Administrador";

        public static List<PerfilItem> ObtenerPerfiles()
        {
            return usuarioDatos.ObtenerPerfiles();
        }

        public static List<UsuarioRow> ObtenerUsuarios()
        {
            return usuarioDatos.ObtenerUsuarios();
        }

        public static List<UsuarioRow> FiltrarUsuarios(List<UsuarioRow> listaCompleta, string filtro)
        {
            if (listaCompleta == null) return new List<UsuarioRow>();
            if (string.IsNullOrWhiteSpace(filtro)) return listaCompleta;

            string[] palabras = filtro.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return listaCompleta.Where(u =>
            {
                string textoCompleto = string.Join(" ", new[]
                {
                    u.Nombre, u.Apellido, u.UsuarioLogin, u.Email, u.NombrePerfil
                }).ToLower();

                return palabras.All(p => textoCompleto.Contains(p));
            }).ToList();
        }

        public static bool ValidarAltaUsuario(string nombre, string apellido, string usuario, string password, string email, int? idPerfil, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(nombre.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                mensajeError = "El nombre solo puede contener letras y espacios.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                mensajeError = "El apellido es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(apellido.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                mensajeError = "El apellido solo puede contener letras y espacios.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                mensajeError = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (usuario.Contains(" "))
            {
                mensajeError = "El nombre de usuario no puede contener espacios.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                mensajeError = "El correo electrónico es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                mensajeError = "El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                mensajeError = "La contraseña debe tener al menos 4 caracteres.";
                return false;
            }

            if (idPerfil == null)
            {
                mensajeError = "Debe seleccionar un perfil.";
                return false;
            }

            if (usuarioDatos.ExisteUsuario(usuario))
            {
                mensajeError = "Ya existe un usuario con ese nombre de usuario.";
                return false;
            }

            if (usuarioDatos.ExisteEmail(email.Trim().ToLower()))
            {
                mensajeError = "Ya existe un usuario registrado con ese correo electrónico.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static void AltaUsuario(string nombre, string apellido, string usuario, string passwordPlano, string email, int idPerfil)
        {
            var nuevoUsuario = new Usuario
            {
                nombre = nombre.Trim(),
                apellido = apellido.Trim(),
                usuario = usuario.Trim(),
                password = SeguridadNegocio.HashPassword(passwordPlano),
                email = email.Trim().ToLower(),
                id_perfil = idPerfil,
                Activo = true
            };

            usuarioDatos.GuardarUsuario(nuevoUsuario);
        }

        public static bool ValidarModificacionUsuario(
            int idUsuario,
            int idUsuarioLogueado,
            int? idPerfilOriginal,
            string nombre,
            string apellido,
            string usuario,
            string email,
            int? idPerfil,
            out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(nombre.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                mensajeError = "El nombre solo puede contener letras y espacios.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                mensajeError = "El apellido es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(apellido.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                mensajeError = "El apellido solo puede contener letras y espacios.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                mensajeError = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                mensajeError = "El formato del correo electrónico no es válido.";
                return false;
            }

            if (idPerfil == null)
            {
                mensajeError = "Debe seleccionar un perfil.";
                return false;
            }

            // Regla de Negocio: Prevención de auto-bloqueo del operador logueado
            if (idUsuario == idUsuarioLogueado && idPerfilOriginal.HasValue && idPerfil.Value != idPerfilOriginal.Value)
            {
                mensajeError = "No podés cambiar tu propio perfil de usuario. Solicítale a otro administrador que realice la gestión.";
                return false;
            }

            if (usuarioDatos.ExisteEmail(email.Trim().ToLower(), idUsuario))
            {
                mensajeError = "Ya existe otro usuario registrado con ese correo electrónico.";
                return false;
            }

            var usuarios = usuarioDatos.ObtenerUsuarios();
            var usuarioActual = usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuarioActual != null
                && usuarioActual.NombrePerfil == PERFIL_ADMINISTRADOR
                && usuarioActual.Activo)
            {
                var perfilNuevo = usuarioDatos.ObtenerPerfiles().FirstOrDefault(p => p.IdPerfil == idPerfil);
                bool dejaDeSerAdmin = perfilNuevo != null && perfilNuevo.NombrePerfil != PERFIL_ADMINISTRADOR;

                if (dejaDeSerAdmin)
                {
                    int adminsActivos = usuarios.Count(u => u.NombrePerfil == PERFIL_ADMINISTRADOR && u.Activo);
                    if (adminsActivos <= 1)
                    {
                        mensajeError = "No se puede quitar el rol de Administrador: debe existir al menos un administrador activo en el sistema.";
                        return false;
                    }
                }
            }

            mensajeError = string.Empty;
            return true;
        }

        public static void ModificarUsuario(int idUsuario, string nombre, string apellido, string usuario, string passwordPlanoOVacio, string email, int idPerfil)
        {
            string hashONulo = string.IsNullOrWhiteSpace(passwordPlanoOVacio)
                ? null
                : SeguridadNegocio.HashPassword(passwordPlanoOVacio);

            usuarioDatos.EjecutarABMUsuario('M',
                idUsuario: idUsuario,
                idPerfil: idPerfil,
                nombre: nombre.Trim(),
                apellido: apellido.Trim(),
                usuario: usuario.Trim(),
                password: hashONulo,
                email: email.Trim().ToLower());
        }

        public static bool ValidarBajaUsuario(int idUsuarioADarDeBaja, int idUsuarioLogueado, out string mensajeError)
        {
            if (idUsuarioADarDeBaja == idUsuarioLogueado)
            {
                mensajeError = "No se puede dar de baja a la cuenta con la que se encuentra iniciada la sesión activa.";
                return false;
            }

            var usuarios = usuarioDatos.ObtenerUsuarios();
            var usuarioObjetivo = usuarios.FirstOrDefault(u => u.IdUsuario == idUsuarioADarDeBaja);

            if (usuarioObjetivo != null
                && usuarioObjetivo.NombrePerfil == PERFIL_ADMINISTRADOR
                && usuarioObjetivo.Activo)
            {
                int adminsActivos = usuarios.Count(u => u.NombrePerfil == PERFIL_ADMINISTRADOR && u.Activo);

                if (adminsActivos <= 1)
                {
                    mensajeError = "No se puede dar de baja: debe existir al menos un administrador activo en el sistema.";
                    return false;
                }
            }

            mensajeError = string.Empty;
            return true;
        }

        public static void DarBajaUsuario(int idUsuario)
        {
            usuarioDatos.EjecutarABMUsuario('B', idUsuario: idUsuario);
        }

        public static void ReactivarUsuario(int idUsuario)
        {
            usuarioDatos.ReactivarUsuario(idUsuario);
        }

        public static int ObtenerIdPorLogin(string login)
        {
            return usuarioDatos.ObtenerIdPorLogin(login);
        }

        // id = 0 y rol = null si el usuario no existe o está inactivo.
        // Si la base no responde lanza excepción (el llamador decide qué hacer).
        public static void ObtenerDatosSesion(string usuario, out int id, out string rol)
        {
            id = 0;
            rol = null;

            const string sql = @"
                SELECT TOP 1 u.id_usuario, p.nombre_perfil
                FROM dbo.Usuario u
                INNER JOIN dbo.Perfiles p ON p.id_perfil = u.id_perfil
                WHERE u.Activo = 1
                  AND (   u.usuario COLLATE Latin1_General_CI_AI = @usuario
                       OR CONCAT(u.nombre, ' ', u.apellido) COLLATE Latin1_General_CI_AI = @usuario)
                ORDER BY u.id_usuario;";

            using (var cn = new ConexionBD().ObtenerConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@usuario", usuario ?? string.Empty);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        id = rd.GetInt32(0);
                        rol = rd.GetString(1);
                    }
                }
            }
        }

    }
}
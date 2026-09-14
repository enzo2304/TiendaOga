using System.Collections.Generic;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    /// <summary>
    /// Reglas de alta y consulta de usuarios. La vista nunca habla
    /// directo con UsuarioDatos: siempre pasa por acá.
    /// </summary>
    public static class UsuarioNegocio
    {
        private static readonly UsuarioDatos usuarioDatos = new UsuarioDatos();

        public static List<PerfilItem> ObtenerPerfiles()
        {
            return usuarioDatos.ObtenerPerfiles();
        }

        public static List<UsuarioRow> ObtenerUsuarios()
        {
            return usuarioDatos.ObtenerUsuarios();
        }

        public static bool ValidarAltaUsuario(string nombre, string apellido, string usuario, string password, int? idPerfil, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                mensajeError = "El apellido es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                mensajeError = "El nombre de usuario es obligatorio.";
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

            mensajeError = string.Empty;
            return true;
        }

        public static void AltaUsuario(string nombre, string apellido, string usuario, string passwordPlano, string email, int idPerfil)
        {
            var nuevoUsuario = new Usuario
            {
                nombre = nombre,
                apellido = apellido,
                usuario = usuario,
                password = SeguridadNegocio.HashPassword(passwordPlano),
                email = email,
                id_perfil = idPerfil,
                Activo = true
            };

            usuarioDatos.GuardarUsuario(nuevoUsuario);
        }

        /// <summary>
        /// Validación para Modificar. A diferencia del Alta, no chequea
        /// nombre de usuario duplicado (el usuario ya existe con ese
        /// nombre) ni exige contraseña (dejarla vacía significa "no cambiar").
        /// </summary>
        public static bool ValidarModificacionUsuario(string nombre, string apellido, string usuario, int? idPerfil, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajeError = "El nombre es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                mensajeError = "El apellido es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                mensajeError = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (idPerfil == null)
            {
                mensajeError = "Debe seleccionar un perfil.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static void ModificarUsuario(int idUsuario, string nombre, string apellido, string usuario, string passwordPlanoOVacio, string email, int idPerfil)
        {
            // Si el campo contraseña quedó vacío, se manda null: el SP
            // conserva la contraseña actual (ISNULL en el UPDATE).
            string hashONulo = string.IsNullOrWhiteSpace(passwordPlanoOVacio)
                ? null
                : SeguridadNegocio.HashPassword(passwordPlanoOVacio);

            usuarioDatos.EjecutarABMUsuario('M',
                idUsuario: idUsuario,
                idPerfil: idPerfil,
                nombre: nombre,
                apellido: apellido,
                usuario: usuario,
                password: hashONulo,
                email: email);
        }

        public static void DarBajaUsuario(int idUsuario)
        {
            usuarioDatos.EjecutarABMUsuario('B', idUsuario: idUsuario);
        }

        public static void ReactivarUsuario(int idUsuario)
        {
            usuarioDatos.ReactivarUsuario(idUsuario);
        }
    }
}
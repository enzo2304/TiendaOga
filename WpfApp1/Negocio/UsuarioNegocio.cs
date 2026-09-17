using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public static bool ValidarAltaUsuario(string nombre, string apellido, string usuario, string password, string email, int? idPerfil, out string mensajeError)
        {
            // 1. Validar Nombre (obligatorio y solo letras/espacios)
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

            // 2. Validar Apellido (obligatorio y solo letras/espacios)
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

            // 3. Validar Usuario (obligatorio y sin espacios)
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

            // 4. Validar Formato de Email (obligatorio y con estructura @ y .)
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

            // 5. Validar Contraseña
            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                mensajeError = "La contraseña debe tener al menos 4 caracteres.";
                return false;
            }

            // 6. Validar Perfil
            if (idPerfil == null)
            {
                mensajeError = "Debe seleccionar un perfil.";
                return false;
            }

            // 7. Validar duplicidad de Login
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

        public static bool ValidarModificacionUsuario(string nombre, string apellido, string usuario, string email, int? idPerfil, out string mensajeError)
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
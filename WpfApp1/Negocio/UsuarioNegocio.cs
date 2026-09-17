using System;
using System.Collections.Generic;
using System.Linq;
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

        private const string PERFIL_ADMINISTRADOR = "Administrador";

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

            // 8. Validar duplicidad de Email
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

        public static bool ValidarModificacionUsuario(int idUsuario, string nombre, string apellido, string usuario, string email, int? idPerfil, out string mensajeError)
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

            // Validar duplicidad de Email, excluyendo al propio usuario que se está editando
            if (usuarioDatos.ExisteEmail(email.Trim().ToLower(), idUsuario))
            {
                mensajeError = "Ya existe otro usuario registrado con ese correo electrónico.";
                return false;
            }

            // Si se le está sacando el rol de Administrador a alguien, verificar
            // que no sea el último administrador activo del sistema.
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

        /// <summary>
        /// Reglas de negocio para dar de baja un usuario:
        /// 1) No podés darte de baja a vos mismo mientras estás logueado.
        /// 2) No puede quedar el sistema sin al menos un administrador activo.
        /// </summary>
        public static bool ValidarBajaUsuario(int idUsuarioADarDeBaja, int idUsuarioLogueado, out string mensajeError)
        {
            if (idUsuarioADarDeBaja == idUsuarioLogueado)
            {
                mensajeError = "No se puede realizar esta accion.";
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
    }
}
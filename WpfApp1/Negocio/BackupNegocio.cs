using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiendaOga.Datos;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class BackupNegocio
    {
        private const string BASE = "TiendaOgaDB";
        private static readonly BackupDatos _datos = new BackupDatos();
        private static readonly BackupArchivoDatos _archivos = new BackupArchivoDatos();

        public static string NombreBase { get { return BASE; } }
        public static string CarpetaPorDefecto { get { return BackupArchivoDatos.CarpetaPorDefecto; } }

        // Nombre sugerido con fecha y hora
        public static string SugerirNombre()
        {
            return $"Backup_Ventas_{DateTime.Now:ddMMyyyy_HHmmss}.bak";
        }

        public static void AsegurarCarpeta(string carpeta)
        {
            try { _archivos.CrearCarpeta(carpeta); } catch { /* se informa al intentar guardar */ }
        }

        // ------------------------------------------------------------
        // BACKUP
        // ------------------------------------------------------------
        public static bool ValidarGeneracion(string rol, string carpeta, string nombre,
                                             out string nombreFinal, out string mensajeError)
        {
            nombreFinal = (nombre ?? string.Empty).Trim();

            if (!PermisosNegocio.PuedeVerBackup(rol))
            {
                mensajeError = "Solo el administrador puede generar copias de seguridad.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(carpeta))
            {
                mensajeError = "Elegí primero una carpeta con el botón \"Examinar...\".";
                return false;
            }

            if (nombreFinal.Length == 0 || nombreFinal.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                mensajeError = "El nombre del archivo está vacío o tiene caracteres no permitidos.";
                return false;
            }

            if (!nombreFinal.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                nombreFinal += ".bak";

            if (nombreFinal.Length > 150)
            {
                mensajeError = "El nombre del archivo es demasiado largo (máximo 150 caracteres).";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static bool ExisteArchivo(string carpeta, string nombreArchivo)
        {
            return _archivos.Existe(Path.Combine(carpeta.Trim(), nombreArchivo));
        }

        public static string Generar(string rol, string usuario, string carpeta, string nombreArchivo)
        {
            if (!PermisosNegocio.PuedeVerBackup(rol))
                throw new UnauthorizedAccessException("Solo el administrador puede generar copias de seguridad.");

            try
            {
                _archivos.CrearCarpeta(carpeta);
                string archivo = _datos.Crear(carpeta, nombreArchivo);
                _archivos.RegistrarAuditoria(usuario, "Backup", "OK", nombreArchivo);
                return archivo;
            }
            catch (Exception ex)
            {
                _archivos.RegistrarAuditoria(usuario, "Backup", "ERROR", ex.Message);
                throw;
            }
        }

        // ------------------------------------------------------------
        // RESTAURACIÓN
        // ------------------------------------------------------------

        // Controla permisos, archivo, base de origen, tipo de backup y versión de SQL Server
        public static bool ValidarRestauracion(string rol, string archivo,
                                               out string mensajeError, out BackupInfo info)
        {
            info = null;

            if (!PermisosNegocio.PuedeVerBackup(rol))
            {
                mensajeError = "Solo el administrador puede restaurar copias de seguridad.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(archivo) || !_archivos.Existe(archivo))
            {
                mensajeError = "El archivo seleccionado no existe. Elegí otro respaldo.";
                return false;
            }

            try
            {
                info = _datos.ObtenerInfo(archivo);
            }
            catch (Exception ex)
            {
                mensajeError = "No se pudo leer el archivo de respaldo:\n" + ex.Message;
                return false;
            }

            if (info == null)
            {
                mensajeError = "El archivo no contiene información de respaldo.";
                return false;
            }

            if (!string.Equals(info.NombreBase, BASE, StringComparison.OrdinalIgnoreCase))
            {
                mensajeError = $"El archivo pertenece a la base \"{info.NombreBase}\", no a {BASE}.";
                return false;
            }

            if (info.Tipo != 1)
            {
                mensajeError = "El archivo no es un backup completo de la base de datos.";
                return false;
            }

            try
            {
                int versionServidor = _datos.ObtenerVersionServidor();
                if (info.VersionMayor > versionServidor)
                {
                    mensajeError = $"El respaldo fue creado con una versión más nueva de SQL Server (v{info.VersionMayor}) que la de este servidor (v{versionServidor}). No se puede restaurar.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                mensajeError = "No se pudo verificar la versión del servidor:\n" + ex.Message;
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        // -1 si no se pudo verificar
        public static int ContarConexiones()
        {
            try { return _datos.ContarConexiones(); }
            catch { return -1; }
        }

        public static void Restaurar(string rol, string usuario, string archivo, bool backupPrevio)
        {
            BackupInfo info;
            string error;
            if (!ValidarRestauracion(rol, archivo, out error, out info))
                throw new InvalidOperationException(error);

            try
            {
                string carpeta = Path.GetDirectoryName(archivo);
                string previo = _datos.Restaurar(archivo, backupPrevio, carpeta);

                string detalle = Path.GetFileName(archivo)
                    + (previo != null ? " - backup previo: " + Path.GetFileName(previo) : string.Empty);
                _archivos.RegistrarAuditoria(usuario, "Restauración", "OK", detalle);
            }
            catch (Exception ex)
            {
                _archivos.RegistrarAuditoria(usuario, "Restauración", "ERROR", ex.Message);
                throw;
            }
        }

        // ------------------------------------------------------------
        // CONSULTAS PARA LA PANTALLA
        // ------------------------------------------------------------
        public static List<BackupArchivo> ListarArchivos(string carpeta)
        {
            return _archivos.ListarBak(carpeta);
        }

        public static List<BackupAuditoria> ListarAuditoria(int cantidad)
        {
            return _archivos.LeerAuditoria()
                .OrderByDescending(a => a.FechaReal)
                .Take(cantidad)
                .ToList();
        }

        public static EstadoBackup ObtenerEstado()
        {
            var estado = new EstadoBackup();

            var ultimo = _archivos.LeerAuditoria()
                .Where(a => a.Operacion == "Backup" && a.Resultado == "OK")
                .OrderByDescending(a => a.FechaReal)
                .FirstOrDefault();

            estado.UltimoBackup = ultimo != null ? (DateTime?)ultimo.FechaReal : null;
            estado.Conectado = _datos.ProbarConexion();

            if (estado.Conectado)
            {
                try { estado.TamanioMb = _datos.ObtenerTamanioMb(); }
                catch { /* se muestra "no disponible" */ }
            }

            return estado;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class BackupArchivoDatos
    {
        public const string CarpetaPorDefecto = @"C:\TiendaOga\Backups";

        private static string RutaLog
        {
            get { return Path.Combine(CarpetaPorDefecto, "auditoria_backup.log"); }
        }

        public bool Existe(string ruta)
        {
            return File.Exists(ruta);
        }

        public void CrearCarpeta(string carpeta)
        {
            Directory.CreateDirectory(carpeta);
        }

        public List<BackupArchivo> ListarBak(string carpeta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(carpeta) || !Directory.Exists(carpeta))
                    return new List<BackupArchivo>();

                return new DirectoryInfo(carpeta).GetFiles("*.bak")
                    .OrderByDescending(f => f.CreationTime)
                    .Select(f => new BackupArchivo
                    {
                        Nombre = f.Name,
                        Ruta = f.FullName,
                        FechaCreacion = f.CreationTime,
                        Bytes = f.Length
                    })
                    .ToList();
            }
            catch
            {
                return new List<BackupArchivo>();   // carpeta inaccesible: se muestra vacío
            }
        }

        public List<BackupAuditoria> LeerAuditoria()
        {
            var lista = new List<BackupAuditoria>();
            try
            {
                if (!File.Exists(RutaLog)) return lista;

                foreach (string linea in File.ReadAllLines(RutaLog))
                {
                    string[] p = linea.Split('|');
                    if (p.Length < 5) continue;

                    DateTime fecha;
                    if (!DateTime.TryParseExact(p[0], "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha)) continue;

                    lista.Add(new BackupAuditoria
                    {
                        FechaReal = fecha,
                        Usuario = p[1],
                        Operacion = p[2],
                        Resultado = p[3],
                        Detalle = p[4]
                    });
                }
            }
            catch { }
            return lista;
        }

        public void RegistrarAuditoria(string usuario, string operacion, string resultado, string detalle)
        {
            try
            {
                Directory.CreateDirectory(CarpetaPorDefecto);
                detalle = (detalle ?? "").Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
                File.AppendAllText(RutaLog,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{usuario}|{operacion}|{resultado}|{detalle}{Environment.NewLine}");
            }
            catch { /* la auditoría nunca debe romper la operación */ }
        }
    }
}
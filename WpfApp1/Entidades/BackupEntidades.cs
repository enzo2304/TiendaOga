using System;

namespace TiendaOga.Entidades
{
    // Fila del historial de archivos .bak (las propiedades coinciden con el XAML)
    public class BackupArchivo
    {
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public DateTime FechaCreacion { get; set; }
        public long Bytes { get; set; }

        public string Fecha { get { return FechaCreacion.ToString("dd/MM/yyyy HH:mm"); } }

        public string Tamano
        {
            get
            {
                double mb = Bytes / 1024.0 / 1024.0;
                return mb >= 1024 ? $"{mb / 1024:N2} GB" : $"{mb:N1} MB";
            }
        }
    }

    // Fila del registro de auditoría
    public class BackupAuditoria
    {
        public DateTime FechaReal { get; set; }
        public string Usuario { get; set; }
        public string Operacion { get; set; }
        public string Resultado { get; set; }
        public string Detalle { get; set; }

        public string Fecha { get { return FechaReal.ToString("dd/MM/yyyy HH:mm"); } }
    }

    // Datos de cabecera de un archivo .bak
    public class BackupInfo
    {
        public string NombreBase { get; set; }
        public int VersionMayor { get; set; }
        public int NivelCompatibilidad { get; set; }
        public int Tipo { get; set; }
        public DateTime FechaBackup { get; set; }
    }

    // Resumen para las líneas de estado de la pantalla
    public class EstadoBackup
    {
        public DateTime? UltimoBackup { get; set; }
        public double? TamanioMb { get; set; }
        public bool Conectado { get; set; }
    }
}
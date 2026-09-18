using System;
using System.Collections.Generic;
using System.Linq;

namespace TiendaOga.Negocio
{
    public static class PermisosNegocio
    {
        // Nombres de rol tal como vienen de nombre_perfil en la base.
        // Si en tu tabla perfil están escritos distinto (mayúsculas, con
        // tildes, etc.) ajustá estas 3 constantes nomás; el resto del
        // archivo no necesita tocarse.
        private const string ROL_GERENTE = "Gerente";
        private const string ROL_ADMINISTRADOR = "Administrador";
        private const string ROL_VENDEDOR = "Vendedor";

        // ------------------------------------------------------------------
        // Matriz de accesos: qué módulo puede ver cada rol.
        // Para agregar un módulo nuevo el día de mañana, se agrega su
        // "Modulo.X" a este enum y se lo suma a la lista del rol que
        // corresponda acá abajo. Todo lo demás (los métodos PuedeVerX de
        // más abajo) sigue funcionando solo.
        // ------------------------------------------------------------------
        private enum Modulo
        {
            Ventas,
            Clientes,
            Productos,
            Usuarios,
            ReportesVendedor,
            ReporteGeneral,
            Backup
        }

        private static readonly Dictionary<string, HashSet<Modulo>> MatrizPermisos =
            new Dictionary<string, HashSet<Modulo>>(StringComparer.OrdinalIgnoreCase)
            {
                // Gerente: ve todos los clientes, los reportes (los mismos
                // que ve el vendedor) y el panel/reporte general.
                [ROL_GERENTE] = new HashSet<Modulo>
                {
                    Modulo.Clientes,
                    Modulo.ReportesVendedor,
                    Modulo.ReporteGeneral
                },

                // Administrador: backup, clientes, y alta de usuarios.
                [ROL_ADMINISTRADOR] = new HashSet<Modulo>
                {
                    Modulo.Backup,
                    Modulo.Clientes,
                    Modulo.Usuarios
                },

                // Vendedor: productos, ventas y clientes.
                [ROL_VENDEDOR] = new HashSet<Modulo>
                {
                    Modulo.Productos,
                    Modulo.Ventas,
                    Modulo.Clientes
                }
            };

        private static bool TienePermiso(string rol, Modulo modulo)
        {
            if (string.IsNullOrWhiteSpace(rol)) return false;
            return MatrizPermisos.TryGetValue(rol.Trim(), out var modulos) && modulos.Contains(modulo);
        }

        // ------------------------------------------------------------------
        // Métodos públicos: uno por botón/módulo del sidebar. Son los que
        // se usan tanto en Window1.ConfigurarVistaPorRol (para mostrar u
        // ocultar el botón) como, idealmente, al principio de cada
        // btnX_Click (para no confiar solo en que el botón esté oculto).
        // ------------------------------------------------------------------
        public static bool PuedeVerVentas(string rol) => TienePermiso(rol, Modulo.Ventas);
        public static bool PuedeVerClientes(string rol) => TienePermiso(rol, Modulo.Clientes);
        public static bool PuedeVerProductos(string rol) => TienePermiso(rol, Modulo.Productos);
        public static bool PuedeVerUsuarios(string rol) => TienePermiso(rol, Modulo.Usuarios);
        public static bool PuedeVerReportesVendedor(string rol) => TienePermiso(rol, Modulo.ReportesVendedor);
        public static bool PuedeVerReporteGeneral(string rol) => TienePermiso(rol, Modulo.ReporteGeneral);
        public static bool PuedeVerBackup(string rol) => TienePermiso(rol, Modulo.Backup);

        // Alias para no romper el código existente: Window1 ya llama a
        // PuedeVerReportesGerencia tanto para "Reportes del vendedor" como
        // para "Panel Gerencial". Lo dejamos apuntando a ReportesVendedor
        // por compatibilidad, pero lo ideal es migrar esas dos llamadas a
        // los métodos específicos de arriba (ya lo hice en Window1 abajo).
        [Obsolete("Usá PuedeVerReportesVendedor o PuedeVerReporteGeneral según el botón.")]
        public static bool PuedeVerReportesGerencia(string rol) => TienePermiso(rol, Modulo.ReportesVendedor);

        // Devuelve el primer módulo visible para un rol, en el orden del
        // sidebar. Sirve para elegir una pantalla inicial cuando el rol no
        // tiene "Panel Principal" propio (ver Window1).
        public static string PrimerModuloVisible(string rol)
        {
            if (PuedeVerVentas(rol)) return nameof(Modulo.Ventas);
            if (PuedeVerClientes(rol)) return nameof(Modulo.Clientes);
            if (PuedeVerProductos(rol)) return nameof(Modulo.Productos);
            if (PuedeVerUsuarios(rol)) return nameof(Modulo.Usuarios);
            if (PuedeVerReportesVendedor(rol)) return nameof(Modulo.ReportesVendedor);
            if (PuedeVerReporteGeneral(rol)) return nameof(Modulo.ReporteGeneral);
            if (PuedeVerBackup(rol)) return nameof(Modulo.Backup);
            return null;
        }
    }
}
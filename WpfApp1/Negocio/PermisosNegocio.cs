using System;
using System.Collections.Generic;

namespace TiendaOga.Negocio
{
    public enum TipoReporteVenta
    {
        Ninguno,
        Individual,
        Consolidado
    }

    public static class PermisosNegocio
    {
        private const string ROL_GERENTE = "Gerente";
        private const string ROL_ADMINISTRADOR = "Administrador";
        private const string ROL_VENDEDOR = "Vendedor";

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
                [ROL_GERENTE] = new HashSet<Modulo>
                {
                    Modulo.Ventas,
                    Modulo.Clientes,
                    Modulo.Productos,
                    Modulo.ReportesVendedor,
                    Modulo.ReporteGeneral
                },

                [ROL_ADMINISTRADOR] = new HashSet<Modulo>
                {
                    Modulo.Backup,
                    Modulo.Clientes,
                    Modulo.Usuarios,
                    Modulo.Ventas
                },

                [ROL_VENDEDOR] = new HashSet<Modulo>
                {
                    Modulo.Productos,
                    Modulo.Ventas,
                    Modulo.Clientes,
                    Modulo.ReportesVendedor
                },
            };

        private static bool TienePermiso(string rol, Modulo modulo)
        {
            if (string.IsNullOrWhiteSpace(rol)) return false;
            return MatrizPermisos.TryGetValue(rol.Trim(), out var modulos) && modulos.Contains(modulo);
        }

        public static bool PuedeVerVentas(string rol) => TienePermiso(rol, Modulo.Ventas);
        public static bool PuedeVerClientes(string rol) => TienePermiso(rol, Modulo.Clientes);
        public static bool PuedeVerProductos(string rol) => TienePermiso(rol, Modulo.Productos);
        public static bool PuedeVerUsuarios(string rol) => TienePermiso(rol, Modulo.Usuarios);
        public static bool PuedeVerReportesVendedor(string rol) => TienePermiso(rol, Modulo.ReportesVendedor);
        public static bool PuedeVerReporteGeneral(string rol) => TienePermiso(rol, Modulo.ReporteGeneral);
        public static bool PuedeVerBackup(string rol) => TienePermiso(rol, Modulo.Backup);

        public static TipoReporteVenta ObtenerAlcanceReporteVenta(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return TipoReporteVenta.Ninguno;

            if (rol.Trim().Equals(ROL_GERENTE, StringComparison.OrdinalIgnoreCase))
                return TipoReporteVenta.Consolidado;

            if (rol.Trim().Equals(ROL_VENDEDOR, StringComparison.OrdinalIgnoreCase))
                return TipoReporteVenta.Individual;

            return TipoReporteVenta.Ninguno;
        }

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
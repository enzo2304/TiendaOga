using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TiendaOga.Entidades;

namespace TiendaOga.Negocio
{
    public static class ProductoNegocio
    {
        public static bool PuedeAdministrarCatalogo(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol)) return false;

            return rol.Equals("Gerente", StringComparison.OrdinalIgnoreCase) ||
                   rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ValidarEliminacion(string rol, out string mensajeError)
        {
            if (!PuedeAdministrarCatalogo(rol))
            {
                mensajeError = "Acceso denegado: El perfil Vendedor solo cuenta con permisos de consulta y no puede dar de baja productos.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static bool ValidarGuardado(string rol, out string mensajeError)
        {
            if (!PuedeAdministrarCatalogo(rol))
            {
                mensajeError = "Acceso denegado: El perfil Vendedor no tiene autorización para dar de alta o modificar productos.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        public static bool ValidarIngresoStock(
            string nombre,
            string txtCosto,
            string txtVenta,
            string txtStock,
            bool esHogar,
            string material,
            string ambiente,
            string marca,
            string modelo,
            string txtGarantia,
            out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensaje = "Debe ingresar el nombre del producto.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCosto) ||
                !decimal.TryParse(txtCosto.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo) || costo <= 0)
            {
                mensaje = "El precio de costo debe ser mayor a 0.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtVenta) ||
                !decimal.TryParse(txtVenta.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal venta) || venta <= 0)
            {
                mensaje = "El precio de venta debe ser mayor a 0.";
                return false;
            }

            if (venta < costo)
            {
                mensaje = "El precio de venta no puede ser menor al precio de costo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtStock) || !int.TryParse(txtStock.Trim(), out int stock) || stock < 0)
            {
                mensaje = "El stock debe ser un número entero mayor o igual a 0.";
                return false;
            }

            if (esHogar)
            {
                if (string.IsNullOrWhiteSpace(material))
                {
                    mensaje = "Indique el material para el producto de Hogar.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(ambiente))
                {
                    mensaje = "Indique el ambiente para el producto de Hogar.";
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(marca))
                {
                    mensaje = "Indique la marca para el producto tecnológico.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(modelo))
                {
                    mensaje = "Indique el modelo para el producto tecnológico.";
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(txtGarantia) && !int.TryParse(txtGarantia.Trim(), out _))
                {
                    mensaje = "La garantía debe ser un número entero de meses.";
                    return false;
                }
            }

            mensaje = string.Empty;
            return true;
        }

        public static List<ProductoRow> ObtenerProductos()
        {
            return DatosGlobales.Productos;
        }

        public static List<ProductoRow> FiltrarProductos(string termino, string categoriaNombre, int idCategoria)
        {
            var consulta = DatosGlobales.Productos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                string term = termino.Trim().ToLower();
                consulta = consulta.Where(p => p.NombreProducto.ToLower().Contains(term) ||
                                               p.IdProducto.ToString().Contains(term));
            }

            if (idCategoria > 0 && !string.IsNullOrWhiteSpace(categoriaNombre))
            {
                consulta = consulta.Where(p => string.Equals(p.NombreCategoria, categoriaNombre, StringComparison.OrdinalIgnoreCase));
            }

            return consulta.ToList();
        }

        public static ProductoRow ObtenerPorId(int idProducto)
        {
            return DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == idProducto);
        }

        public static void GuardarOModificarProducto(
            int? idProducto,
            string nombre,
            decimal costo,
            decimal venta,
            int stock,
            string tipoProducto)
        {
            if (!idProducto.HasValue || idProducto.Value == 0)
            {
                int nuevoId = DatosGlobales.Productos.Count > 0
                    ? DatosGlobales.Productos.Max(p => p.IdProducto) + 1
                    : 1;

                DatosGlobales.Productos.Add(new ProductoRow
                {
                    IdProducto = nuevoId,
                    NombreProducto = nombre,
                    PrecioCosto = costo,
                    PrecioVentas = venta,
                    Stock = stock,
                    TipoProducto = tipoProducto,
                    NombreCategoria = tipoProducto,
                    Activo = true
                });
            }
            else
            {
                var prod = DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == idProducto.Value);
                if (prod != null)
                {
                    prod.NombreProducto = nombre;
                    prod.PrecioCosto = costo;
                    prod.PrecioVentas = venta;
                    prod.Stock = stock;
                    prod.TipoProducto = tipoProducto;
                    prod.NombreCategoria = tipoProducto;
                }
            }
        }

        public static bool CambiarEstadoActivo(int idProducto, bool nuevoEstado)
        {
            var prod = DatosGlobales.Productos.FirstOrDefault(p => p.IdProducto == idProducto);
            if (prod == null) return false;

            prod.Activo = nuevoEstado;
            return true;
        }
    }
}
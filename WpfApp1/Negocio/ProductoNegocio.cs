using System;

namespace TiendaOga.Negocio
{
    public static class ProductoNegocio
    {
        public static bool ValidarIngresoStock(
            string nombre,
            int indiceCategoria,
            string txtCosto,
            string txtVenta,
            string txtStock,
            bool esHogar,
            string material,
            string ambiente,
            string marca,
            string modelo,
            out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensaje = "Debe ingresar el nombre del producto.";
                return false;
            }

            if (indiceCategoria == -1)
            {
                mensaje = "Seleccione una categoría.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCosto) || !decimal.TryParse(txtCosto.Replace('.', ','), out decimal costo) || costo <= 0)
            {
                mensaje = "El precio de costo debe ser mayor a 0.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtVenta) || !decimal.TryParse(txtVenta.Replace('.', ','), out decimal venta) || venta <= 0)
            {
                mensaje = "El precio de venta debe ser mayor a 0.";
                return false;
            }

            if (venta < costo)
            {
                mensaje = "El precio de venta no puede ser menor al precio de costo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtStock) || !int.TryParse(txtStock, out int stock) || stock <= 0)
            {
                mensaje = "La cantidad a ingresar debe ser un número entero mayor a 0.";
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
            }

            mensaje = string.Empty;
            return true;
        }
    }
}
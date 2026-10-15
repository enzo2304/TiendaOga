using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class ProductoVentaDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        // Para la pantalla de ventas (solo productos disponibles y activos)
        public List<ProductoRow> ListarActivos()
        {
            var lista = new List<ProductoRow>();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_listar_activos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new ProductoRow
                        {
                            IdProducto = rd.GetInt32(0),
                            NombreProducto = rd.GetString(1),
                            PrecioVentas = rd.GetDecimal(2),
                            Stock = rd.GetInt32(3),
                            Activo = true
                        });
                    }
                }
            }
            return lista;
        }

        // Para la pantalla de Productos (Catálogo completo: activos e inactivos)
        public List<ProductoRow> ListarCatalogoCompleto()
        {
            var lista = new List<ProductoRow>();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_listar_catalogo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new ProductoRow
                        {
                            IdProducto = rd.GetInt32(rd.GetOrdinal("id_producto")),
                            NombreProducto = rd["nombre_producto"].ToString(),
                            NombreCategoria = rd["nombre_categoria"].ToString(),
                            PrecioCosto = Convert.ToDecimal(rd["precio_costo"]),
                            PrecioVentas = Convert.ToDecimal(rd["precio_ventas"]),
                            Stock = Convert.ToInt32(rd["stock"]),
                            TipoProducto = rd["tipo_producto"].ToString(),
                            Activo = Convert.ToBoolean(rd["activo"])
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener un producto por ID con todos sus campos
        public ProductoRow ObtenerPorId(int id)
        {
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_obtener_detalle", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_producto", id);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new ProductoRow
                        {
                            IdProducto = rd.GetInt32(rd.GetOrdinal("id_producto")),
                            NombreProducto = rd["nombre_producto"].ToString(),
                            NombreCategoria = rd["nombre_categoria"].ToString(),
                            PrecioCosto = Convert.ToDecimal(rd["precio_costo"]),
                            PrecioVentas = Convert.ToDecimal(rd["precio_ventas"]),
                            Stock = Convert.ToInt32(rd["stock"]),
                            TipoProducto = rd["tipo_producto"].ToString(),
                            Activo = Convert.ToBoolean(rd["activo"])
                        };
                    }
                    return null;
                }
            }
        }

        // Guardar o Modificar en la Base de Datos
        public int GuardarOModificar(
            int? idProducto,
            string nombre,
            decimal costo,
            decimal venta,
            int stock,
            string tipoProducto,
            string material = null,
            string dimensiones = null,
            decimal? peso = null,
            string ambiente = null,
            string marca = null,
            string modelo = null,
            int? garantia = null,
            string voltaje = null)
        {
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_guardar_o_modificar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_producto", (object)idProducto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@precio_costo", costo);
                cmd.Parameters.AddWithValue("@precio_ventas", venta);
                cmd.Parameters.AddWithValue("@stock", stock);
                cmd.Parameters.AddWithValue("@tipo_producto", tipoProducto);

                // Parámetros Hogar
                cmd.Parameters.AddWithValue("@material", (object)material ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@dimensiones", (object)dimensiones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@peso", (object)peso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ambiente", (object)ambiente ?? DBNull.Value);

                // Parámetros Tecnología
                cmd.Parameters.AddWithValue("@marca", (object)marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@modelo", (object)modelo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@garantia_meses", (object)garantia ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@voltaje", (object)voltaje ?? DBNull.Value);

                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Baja lógica o reactivación
        public bool CambiarEstadoActivo(int idProducto, bool nuevoEstado)
        {
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_cambiar_activo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_producto", idProducto);
                cmd.Parameters.AddWithValue("@activo", nuevoEstado);

                cn.Open();
                int filas = cmd.ExecuteNonQuery();
                return filas > 0;
            }
        }
    }
}
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class ProductoVentaDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        public List<ProductoRow> ListarActivos()
        {
            var lista = new List<ProductoRow>();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_listar_activos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) lista.Add(Mapear(rd));
            }
            return lista;
        }

        public ProductoRow ObtenerPorId(int id)
        {
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_producto_obtener", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_producto", id);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    return rd.Read() ? Mapear(rd) : null;
            }
        }

        private static ProductoRow Mapear(SqlDataReader rd)
        {
            return new ProductoRow
            {
                IdProducto = rd.GetInt32(0),
                NombreProducto = rd.GetString(1),
                PrecioVentas = rd.GetDecimal(2),
                Stock = rd.GetInt32(3)
            };
        }
    }
}
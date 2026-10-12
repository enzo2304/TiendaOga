using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class VentaDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        public VentaRegistrada Registrar(int idUsuario, int idCliente, int idTipoPago,
                                         IEnumerable<ItemVenta> detalle)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("id_producto", typeof(int));
            tabla.Columns.Add("cantidad", typeof(int));
            foreach (var item in detalle)
                tabla.Rows.Add(item.IdProducto, item.Cantidad);

            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_venta_registrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                cmd.Parameters.AddWithValue("@id_tipo_pago", idTipoPago);

                var p = cmd.Parameters.Add("@detalle", SqlDbType.Structured);
                p.TypeName = "dbo.tipo_detalle_venta";
                p.Value = tabla;

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    rd.Read();
                    return new VentaRegistrada
                    {
                        IdVenta = rd.GetInt32(0),
                        NumeroVenta = rd.GetString(1),
                        Total = rd.GetDecimal(2)
                    };
                }
            }
        }
    }
}
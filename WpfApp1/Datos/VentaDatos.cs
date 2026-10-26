using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
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

        public List<VentaTurnoItem> ObtenerVentasDelDiaPorVendedor(int idUsuario)
        {
            var lista = new List<VentaTurnoItem>();

            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_ventas_del_dia_por_vendedor", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new VentaTurnoItem
                        {
                            NroComprobante = rd["NroComprobante"] != DBNull.Value ? rd["NroComprobante"].ToString() : string.Empty,
                            Hora = rd["Hora"] != DBNull.Value ? rd["Hora"].ToString() : string.Empty,
                            NombreCliente = rd["NombreCliente"] != DBNull.Value ? rd["NombreCliente"].ToString() : string.Empty,
                            Detalle = rd["Detalle"] != DBNull.Value ? rd["Detalle"].ToString() : string.Empty,
                            MetodoPago = rd["MetodoPago"] != DBNull.Value ? rd["MetodoPago"].ToString() : "Sin pago",
                            Total = rd["Total"] != DBNull.Value ? Convert.ToDecimal(rd["Total"]) : 0m
                        });
                    }
                }
            }

            return lista;
        }

        public void AnularVenta(int idVenta, int idUsuarioAnulo)
        {
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_anular_venta", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_venta", idVenta);
                cmd.Parameters.AddWithValue("@id_usuario_anulo", idUsuarioAnulo);

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
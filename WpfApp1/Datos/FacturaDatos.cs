using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class FacturaDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        public Factura Obtener(int idVenta)
        {
            var f = new Factura { IdVenta = idVenta };

            using (var cn = _bd.ObtenerConexion())
            {
                cn.Open();

                using (var cmd = Comando(cn, "dbo.sp_factura_cabecera", idVenta))
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    f.NumeroVenta = rd.GetString(rd.GetOrdinal("numero_venta"));
                    f.FechaVenta = rd.GetDateTime(rd.GetOrdinal("fecha_venta"));
                    f.TotalVenta = rd.GetDecimal(rd.GetOrdinal("total_venta"));
                    f.Vendedor = rd.GetString(rd.GetOrdinal("vendedor"));
                    f.NombreCliente = rd.GetString(rd.GetOrdinal("nombre_razon_social"));
                    f.TipoDocumento = rd.GetString(rd.GetOrdinal("tipo_documento"));
                    f.NroDocumento = rd.GetString(rd.GetOrdinal("nro_documento"));
                    f.Estado = rd.GetString(rd.GetOrdinal("estado"));
                }

                using (var cmd = Comando(cn, "dbo.sp_factura_detalle", idVenta))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        f.Items.Add(new FacturaItem
                        {
                            IdProducto = rd.GetInt32(rd.GetOrdinal("id_producto")),
                            Nombre = rd.GetString(rd.GetOrdinal("nombre")),
                            Cantidad = rd.GetInt32(rd.GetOrdinal("cantidad")),
                            PrecioUnitario = rd.GetDecimal(rd.GetOrdinal("precio_unitario")),
                            SubTotal = rd.GetDecimal(rd.GetOrdinal("sub_total"))
                        });
                }

                using (var cmd = Comando(cn, "dbo.sp_factura_pagos", idVenta))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        f.Pagos.Add(new FacturaPago
                        {
                            TipoPago = rd.GetString(rd.GetOrdinal("nombre_tipo_pago")),
                            Monto = rd.GetDecimal(rd.GetOrdinal("monto"))
                        });
                }
            }
            return f;
        }

        private static SqlCommand Comando(SqlConnection cn, string procedimiento, int idVenta)
        {
            var cmd = new SqlCommand(procedimiento, cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@id_venta", idVenta);
            return cmd;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class ReporteDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        public List<ReporteProductoRow> ObtenerProductosMasVendidos(int mes, int anio)
        {
            var lista = new List<ReporteProductoRow>();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_reporte_productos_mas_vendidos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mes", mes);
                cmd.Parameters.AddWithValue("@anio", anio);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new ReporteProductoRow
                        {
                            Posicion = Convert.ToInt32(rd["Posicion"]),
                            IdProducto = Convert.ToInt32(rd["id_producto"]),
                            NombreProducto = rd["NombreProducto"].ToString(),
                            Categoria = rd["Categoria"].ToString(),
                            CantidadVendida = Convert.ToInt32(rd["CantidadVendida"]),
                            TotalIngreso = Convert.ToDecimal(rd["TotalIngreso"]),
                            StockActual = Convert.ToInt32(rd["StockActual"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}
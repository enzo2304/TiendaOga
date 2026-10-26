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

        public ReporteGeneralDTO ObtenerReporteGeneral(int mes, int anio)
        {
            var dto = new ReporteGeneralDTO();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_reporte_productos_mas_vendidos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mes", mes);
                cmd.Parameters.AddWithValue("@anio", anio);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    // Bloque 1: Ranking de Productos
                    while (rd.Read())
                    {
                        dto.Productos.Add(new ReporteProductoRow
                        {
                            Posicion = Convert.ToInt32(rd["Posicion"]),
                            IdProducto = Convert.ToInt32(rd["id_producto"]),
                            NombreProducto = rd["NombreProducto"].ToString(),
                            Categoria = rd["Categoria"].ToString(),
                            CantidadVendida = Convert.ToInt32(rd["CantidadVendida"]),
                            TotalIngreso = Convert.ToDecimal(rd["TotalIngreso"]),
                            CostoTotal = Convert.ToDecimal(rd["CostoTotal"]),
                            GananciaNeta = Convert.ToDecimal(rd["GananciaNeta"]),
                            StockActual = Convert.ToInt32(rd["StockActual"])
                        });
                    }

                    // Bloque 2: Métodos de Pago Generales
                    if (rd.NextResult())
                    {
                        while (rd.Read())
                        {
                            dto.MetodosDePagoGenerales.Add(new PagoRendimiento
                            {
                                Metodo = rd["MetodoPago"].ToString(),
                                Total = Convert.ToDecimal(rd["TotalCobrado"])
                            });
                        }
                    }

                    // Bloque 3: Métodos de Pago por Producto
                    if (rd.NextResult())
                    {
                        while (rd.Read())
                        {
                            dto.PagosPorProducto.Add(new PagoPorProducto
                            {
                                IdProducto = Convert.ToInt32(rd["id_producto"]),
                                Metodo = rd["MetodoPago"].ToString(),
                                Total = Convert.ToDecimal(rd["TotalCobrado"])
                            });
                        }
                    }
                }
            }
            return dto;
        }
    }
}
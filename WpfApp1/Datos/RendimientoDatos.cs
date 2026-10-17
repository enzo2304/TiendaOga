using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class RendimientoDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        public RendimientoVendedorDTO ObtenerRendimiento(int idUsuario, DateTime desde, DateTime hasta)
        {
            var dto = new RendimientoVendedorDTO();

            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_reporte_rendimiento_vendedor", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@fecha_desde", desde);
                cmd.Parameters.AddWithValue("@fecha_hasta", hasta);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    // BLOQUE 1: KPIs
                    if (rd.Read())
                    {
                        dto.CantidadVentas = Convert.ToInt32(rd["CantidadVentas"]);
                        dto.VolumenFacturacion = Convert.ToDecimal(rd["VolumenFacturacion"]);
                        dto.TicketPromedio = Convert.ToDecimal(rd["TicketPromedio"]);
                        dto.CantidadAnuladas = Convert.ToInt32(rd["CantidadAnuladas"]);
                        dto.TasaAnulacion = Convert.ToDouble(rd["TasaAnulacion"]);
                    }

                    // BLOQUE 2: Métodos de Pago
                    if (rd.NextResult())
                    {
                        while (rd.Read())
                        {
                            dto.MetodosDePago.Add(new PagoRendimiento
                            {
                                Metodo = rd["MetodoPago"].ToString(),
                                Total = Convert.ToDecimal(rd["TotalCobrado"])
                            });
                        }
                    }

                    // BLOQUE 3: Top 5 Productos
                    if (rd.NextResult())
                    {
                        while (rd.Read())
                        {
                            dto.TopProductos.Add(new ProductoRendimiento
                            {
                                Nombre = rd["NombreProducto"].ToString(),
                                Cantidad = Convert.ToInt32(rd["CantidadVendida"]),
                                Subtotal = Convert.ToDecimal(rd["SubtotalGenerado"])
                            });
                        }
                    }
                }
            }
            return dto;
        }

        // Método para llenar el ComboBox de selección de vendedor
        public List<dynamic> ObtenerVendedoresActivos()
        {
            var lista = new List<dynamic>();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("SELECT id_usuario, CONCAT(nombre, ' ', apellido, ' (', usuario, ')') AS NombreCompleto FROM dbo.Usuario WHERE activo = 1", cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new
                        {
                            IdUsuario = Convert.ToInt32(rd["id_usuario"]),
                            NombreCompleto = rd["NombreCompleto"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}
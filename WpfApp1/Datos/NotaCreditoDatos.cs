using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TiendaOga.Datos
{
    public class NotaCreditoDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        // Devuelve null si la venta no existe o no está anulada
        public bool ObtenerExtra(int idVenta, out DateTime fechaAnulacion, out string anulador)
        {
            fechaAnulacion = DateTime.Now;
            anulador = "-";

            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_nota_credito_extra", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_venta", idVenta);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return false;

                    int iFecha = rd.GetOrdinal("fecha_anulacion");
                    if (!rd.IsDBNull(iFecha)) fechaAnulacion = rd.GetDateTime(iFecha);
                    anulador = rd.GetString(rd.GetOrdinal("anulador"));
                    return true;
                }
            }
        }
    }
}
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class TipoPagoDatos
    {
        private readonly ConexionBD _bd = new ConexionBD();

        public List<TipoPagoItem> Listar()
        {
            var lista = new List<TipoPagoItem>();
            using (var cn = _bd.ObtenerConexion())
            using (var cmd = new SqlCommand("dbo.sp_tipo_pago_listar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        lista.Add(new TipoPagoItem
                        {
                            IdTipoPago = rd.GetInt32(0),
                            Nombre = rd.GetString(1)
                        });
            }
            return lista;
        }
    }
}
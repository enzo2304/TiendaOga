using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TiendaOga.Entidades;

namespace TiendaOga.Datos
{
    public class ClienteDatos
    {
        // ConexionBD no es estatica: se crea una instancia
        private readonly ConexionBD _conexion = new ConexionBD();

        // ---------------------------------------------------------------
        // LISTAR (con cantidad de compras)
        // ---------------------------------------------------------------
        public List<ClienteRow> Listar(bool soloActivos = false)
        {
            var lista = new List<ClienteRow>();

            using (var cn = _conexion.ObtenerConexion())
            using (var cmd = new SqlCommand("sp_cliente_listar_con_compras", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@solo_activos", soloActivos);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new ClienteRow
                        {
                            IdCliente = rd.GetInt32(rd.GetOrdinal("id_cliente")),
                            TipoCliente = rd.GetString(rd.GetOrdinal("tipo_cliente")),
                            NombreRazonSocial = rd.GetString(rd.GetOrdinal("nombre_razon_social")),
                            TipoDocumento = rd.GetString(rd.GetOrdinal("tipo_documento")),
                            NroDocumento = rd.GetString(rd.GetOrdinal("nro_documento")),
                            Telefono = rd.IsDBNull(rd.GetOrdinal("telefono")) ? "" : rd.GetString(rd.GetOrdinal("telefono")),
                            Activo = rd.GetBoolean(rd.GetOrdinal("activo")),
                            Compras = rd.GetInt32(rd.GetOrdinal("compras"))
                        });
                    }
                }
            }

            return lista;
        }

        // ---------------------------------------------------------------
        // INSERTAR (devuelve el id del nuevo cliente)
        // ---------------------------------------------------------------
        public int Insertar(string tipoCliente, string nombreRazonSocial,
                            string tipoDocumento, string nroDocumento, string telefono)
        {
            try
            {
                using (var cn = _conexion.ObtenerConexion())
                using (var cmd = new SqlCommand("sp_cliente_insertar", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tipo_cliente", tipoCliente);
                    cmd.Parameters.AddWithValue("@nombre_razon_social", nombreRazonSocial);
                    cmd.Parameters.AddWithValue("@tipo_documento", tipoDocumento);
                    cmd.Parameters.AddWithValue("@nro_documento", nroDocumento);
                    cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                    cn.Open();

                    // SCOPE_IDENTITY() llega como decimal, por eso Convert
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new InvalidOperationException("Ya existe un cliente con ese tipo y número de documento.");
            }
        }

        // ---------------------------------------------------------------
        // MODIFICAR
        // ---------------------------------------------------------------
        public void Modificar(int idCliente, string tipoCliente, string nombreRazonSocial,
                              string tipoDocumento, string nroDocumento, string telefono)
        {
            try
            {
                using (var cn = _conexion.ObtenerConexion())
                using (var cmd = new SqlCommand("sp_cliente_modificar", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                    cmd.Parameters.AddWithValue("@tipo_cliente", tipoCliente);
                    cmd.Parameters.AddWithValue("@nombre_razon_social", nombreRazonSocial);
                    cmd.Parameters.AddWithValue("@tipo_documento", tipoDocumento);
                    cmd.Parameters.AddWithValue("@nro_documento", nroDocumento);
                    cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                throw new InvalidOperationException("Ya existe otro cliente con ese tipo y número de documento.");
            }
        }

        // ---------------------------------------------------------------
        // BAJA LOGICA / REACTIVAR
        // ---------------------------------------------------------------
        public void Baja(int idCliente)
        {
            EjecutarPorId("sp_cliente_baja", idCliente);
        }

        public void Reactivar(int idCliente)
        {
            EjecutarPorId("sp_cliente_reactivar", idCliente);
        }

        private void EjecutarPorId(string procedimiento, int idCliente)
        {
            using (var cn = _conexion.ObtenerConexion())
            using (var cmd = new SqlCommand(procedimiento, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------------------------------------------------------------
        // HISTORIAL DE COMPRAS DE UN CLIENTE
        // ---------------------------------------------------------------
        public List<CompraRow> ObtenerHistorial(int idCliente)
        {
            var lista = new List<CompraRow>();

            using (var cn = _conexion.ObtenerConexion())
            using (var cmd = new SqlCommand("sp_cliente_historial", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista.Add(new CompraRow
                        {
                            NumeroVenta = rd.GetString(rd.GetOrdinal("numero_venta")),
                            FechaVenta = rd.GetDateTime(rd.GetOrdinal("fecha_venta")),
                            Articulos = rd.IsDBNull(rd.GetOrdinal("articulos")) ? "" : rd.GetString(rd.GetOrdinal("articulos")),
                            MetodoPago = rd.IsDBNull(rd.GetOrdinal("metodo_pago")) ? "Sin pagos" : rd.GetString(rd.GetOrdinal("metodo_pago")),
                            TotalVenta = rd.GetDecimal(rd.GetOrdinal("total_venta"))
                        });
                    }
                }
            }

            return lista;
        }
    }
}
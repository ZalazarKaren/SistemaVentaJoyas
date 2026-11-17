using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDatos
{
    /// <summary>
    /// Consulta los registros de auditoría e historial de ventas y compras
    /// almacenados en SQL Server.
    /// </summary>
    public class CD_Historico
    {
        /// <summary>
        /// Recupera el historial de ventas registradas dentro de un rango de fechas.
        /// </summary>
        public List<HistoricoVenta> ListarVentas(string fechaInicio, string fechaFin)
        {
            List<HistoricoVenta> lista = new List<HistoricoVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // 1. Armar consulta reemplazando nulos en datos del cliente y formateando fecha (dd/MM/yyyy HH:mm)
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("SELECT IdHistoricoVenta, IdVenta, TipoDocumento, NumeroDocumento,");
                    query.AppendLine("ISNULL(DocumentoCliente, '0') AS DocumentoCliente,");
                    query.AppendLine("ISNULL(NombreCliente, 'Consumidor Final') AS NombreCliente,");
                    query.AppendLine("IdUsuario, UsuarioRegistro, MontoPago, MontoCambio, MontoTotal, Accion,");
                    query.AppendLine("CONVERT(VARCHAR(19), FechaHistorico, 103) + ' ' + CONVERT(VARCHAR(5), FechaHistorico, 108) AS FechaHistorico");
                    query.AppendLine("FROM dbo.HISTORICO_VENTA");
                    query.AppendLine("WHERE CONVERT(DATE, FechaHistorico) BETWEEN CONVERT(DATE, @fechainicio, 103) AND CONVERT(DATE, @fechafin, 103)");
                    query.AppendLine("ORDER BY IdHistoricoVenta DESC");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@fechainicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechafin", fechaFin);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    // 2. Leer los resultados y mapear cada registro a la entidad HistoricoVenta
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new HistoricoVenta()
                            {
                                IdHistoricoVenta = Convert.ToInt32(dr["IdHistoricoVenta"]),
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                TipoDocumento = dr["TipoDocumento"].ToString(),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                DocumentoCliente = dr["DocumentoCliente"].ToString(),
                                NombreCliente = dr["NombreCliente"].ToString(),
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                MontoPago = Convert.ToDecimal(dr["MontoPago"]),
                                MontoCambio = Convert.ToDecimal(dr["MontoCambio"]),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"]),
                                Accion = dr["Accion"].ToString(),
                                FechaHistorico = dr["FechaHistorico"].ToString()
                            });
                        }
                    }
                }
                catch
                {
                    lista = new List<HistoricoVenta>();
                }
            }

            return lista;
        }

        /// <summary>
        /// Recupera el historial de compras registradas dentro de un rango de fechas.
        /// </summary>
        public List<HistoricoCompra> ListarCompras(string fechaInicio, string fechaFin)
        {
            List<HistoricoCompra> lista = new List<HistoricoCompra>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // 1. Armar consulta reemplazando nulos en proveedor y formateando fecha (dd/MM/yyyy HH:mm)
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("SELECT IdHistoricoCompra, IdCompra, TipoDocumento, NumeroDocumento,");
                    query.AppendLine("IdProveedor, ISNULL(DocumentoProveedor, '') AS DocumentoProveedor,");
                    query.AppendLine("ISNULL(RazonSocialProveedor, '') AS RazonSocialProveedor,");
                    query.AppendLine("IdUsuario, UsuarioRegistro, MontoTotal, Accion,");
                    query.AppendLine("CONVERT(VARCHAR(19), FechaHistorico, 103) + ' ' + CONVERT(VARCHAR(5), FechaHistorico, 108) AS FechaHistorico");
                    query.AppendLine("FROM dbo.HISTORICO_COMPRA");
                    query.AppendLine("WHERE CONVERT(DATE, FechaHistorico) BETWEEN CONVERT(DATE, @fechainicio, 103) AND CONVERT(DATE, @fechafin, 103)");
                    query.AppendLine("ORDER BY IdHistoricoCompra DESC");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@fechainicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fechafin", fechaFin);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    // 2. Leer los resultados y mapear cada registro a la entidad HistoricoCompra
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new HistoricoCompra()
                            {
                                IdHistoricoCompra = Convert.ToInt32(dr["IdHistoricoCompra"]),
                                IdCompra = Convert.ToInt32(dr["IdCompra"]),
                                TipoDocumento = dr["TipoDocumento"].ToString(),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                IdProveedor = Convert.ToInt32(dr["IdProveedor"]),
                                DocumentoProveedor = dr["DocumentoProveedor"].ToString(),
                                RazonSocialProveedor = dr["RazonSocialProveedor"].ToString(),
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"]),
                                Accion = dr["Accion"].ToString(),
                                FechaHistorico = dr["FechaHistorico"].ToString()
                            });
                        }
                    }
                }
                catch
                {
                    lista = new List<HistoricoCompra>();
                }
            }

            return lista;
        }
    }
}
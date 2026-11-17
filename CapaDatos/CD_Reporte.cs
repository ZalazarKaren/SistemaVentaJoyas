using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    /// <summary>
    /// Ejecuta las consultas SQL para generar los reportes resumidos
    /// de compras y ventas filtrados por rango de fechas y criterios opcionales.
    /// </summary>
    public class CD_Reporte
    {
        /// <summary>
        /// Obtiene el reporte resumido de compras entre dos fechas, permitiendo filtrar
        /// por un proveedor específico o traer todos si idproveedor es 0.
        /// </summary>
        public List<ReporteCompra> CompraSinDetalle(string fechainicio, string fechafin, int idproveedor)
        {
            List<ReporteCompra> lista = new List<ReporteCompra>();
            // Cultura argentina para formatear montos con separador de miles (ej. 15.000)
            CultureInfo culturaAR = new CultureInfo("es-AR");

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // 1. Construir consulta enlazando proveedor y usuario que registró cada compra
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("SELECT");
                    query.AppendLine("    CONVERT(VARCHAR(10), c.FechaRegistro, 103) AS FechaRegistro,");
                    query.AppendLine("    c.TipoDocumento,");
                    query.AppendLine("    c.NumeroDocumento,");
                    query.AppendLine("    c.MontoTotal,");
                    query.AppendLine("    ISNULL(pu.Nombre + ' ' + pu.Apellido, 'Usuario Sistema') AS UsuarioRegistro,");
                    query.AppendLine("    ISNULL(pp.Documento, '') AS DocumentoProveedor,");
                    query.AppendLine("    ISNULL(pr.RazonSocial, '') AS RazonSocial");
                    query.AppendLine("FROM dbo.COMPRA c");
                    query.AppendLine("INNER JOIN dbo.PROVEEDOR pr ON pr.IdProveedor = c.IdProveedor");
                    query.AppendLine("LEFT JOIN dbo.PERSONA pp ON pp.IdPersona = pr.IdPersona");
                    query.AppendLine("LEFT JOIN dbo.USUARIO u ON u.IdUsuario = c.IdUsuario");
                    query.AppendLine("LEFT JOIN dbo.PERSONA pu ON pu.IdPersona = u.IdPersona");
                    query.AppendLine("WHERE CONVERT(DATE, c.FechaRegistro) BETWEEN CONVERT(DATE, @fechainicio, 103) AND CONVERT(DATE, @fechafin, 103)");
                    // Si @idproveedor es 0 se omiten filtros de proveedor y se devuelven todos
                    query.AppendLine("  AND (@idproveedor = 0 OR c.IdProveedor = @idproveedor)");
                    query.AppendLine("ORDER BY c.IdCompra DESC");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@fechainicio", fechainicio);
                    cmd.Parameters.AddWithValue("@fechafin", fechafin);
                    cmd.Parameters.AddWithValue("@idproveedor", idproveedor);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    // 2. Leer filas y mapear cada registro aplicando formato numérico local
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteCompra()
                            {
                                FechaRegistro = dr["FechaRegistro"].ToString(),
                                TipoDocumento = dr["TipoDocumento"].ToString(),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"]).ToString("N0", culturaAR),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                DocumentoProveedor = dr["DocumentoProveedor"].ToString(),
                                RazonSocial = dr["RazonSocial"].ToString()
                            });
                        }
                    }
                }
                catch
                {
                    lista = new List<ReporteCompra>();
                }
            }

            return lista;
        }

        /// <summary>
        /// Obtiene el reporte resumido de ventas entre dos fechas, permitiendo filtrar
        /// por tipo de comprobante o incluir todos cuando tipodocumento es 'TODOS'.
        /// </summary>
        public List<ReporteVenta> VentaSinDetalle(string fechainicio, string fechafin, string tipodocumento)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();
            CultureInfo culturaAR = new CultureInfo("es-AR");

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // 1. Construir consulta de ventas reemplazando nulos en datos de cliente y usuario
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("SELECT");
                    query.AppendLine("    CONVERT(VARCHAR(10), v.FechaRegistro, 103) AS FechaRegistro,");
                    query.AppendLine("    v.TipoDocumento,");
                    query.AppendLine("    v.NumeroDocumento,");
                    query.AppendLine("    ISNULL(v.DocumentoCliente, '0') AS DocumentoCliente,");
                    query.AppendLine("    ISNULL(v.NombreCliente, 'Consumidor Final') AS NombreCliente,");
                    query.AppendLine("    ISNULL(pu.Nombre + ' ' + pu.Apellido, 'Usuario Sistema') AS UsuarioRegistro,");
                    query.AppendLine("    v.MontoPago,");
                    query.AppendLine("    v.MontoCambio,");
                    query.AppendLine("    v.MontoTotal");
                    query.AppendLine("FROM dbo.VENTA v");
                    query.AppendLine("LEFT JOIN dbo.USUARIO u ON u.IdUsuario = v.IdUsuario");
                    query.AppendLine("LEFT JOIN dbo.PERSONA pu ON pu.IdPersona = u.IdPersona");
                    query.AppendLine("WHERE CONVERT(DATE, v.FechaRegistro) BETWEEN CONVERT(DATE, @fechainicio, 103) AND CONVERT(DATE, @fechafin, 103)");
                    // Si @tipodocumento es 'TODOS' incluye Boletas, Facturas y demás comprobantes
                    query.AppendLine("  AND (@tipodocumento = 'TODOS' OR v.TipoDocumento = @tipodocumento)");
                    query.AppendLine("ORDER BY v.IdVenta DESC");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.Parameters.AddWithValue("@fechainicio", fechainicio);
                    cmd.Parameters.AddWithValue("@fechafin", fechafin);
                    cmd.Parameters.AddWithValue("@tipodocumento", tipodocumento);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    // 2. Mapear los resultados formateando pago, vuelto y total en pesos argentinos
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteVenta()
                            {
                                FechaRegistro = dr["FechaRegistro"].ToString(),
                                TipoDocumento = dr["TipoDocumento"].ToString(),
                                NumeroDocumento = dr["NumeroDocumento"].ToString(),
                                DocumentoCliente = dr["DocumentoCliente"].ToString(),
                                NombreCliente = dr["NombreCliente"].ToString(),
                                UsuarioRegistro = dr["UsuarioRegistro"].ToString(),
                                MontoPago = Convert.ToDecimal(dr["MontoPago"]).ToString("N0", culturaAR),
                                MontoCambio = Convert.ToDecimal(dr["MontoCambio"]).ToString("N0", culturaAR),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"]).ToString("N0", culturaAR)
                            });
                        }
                    }
                }
                catch
                {
                    lista = new List<ReporteVenta>();
                }
            }

            return lista;
        }
    }
}
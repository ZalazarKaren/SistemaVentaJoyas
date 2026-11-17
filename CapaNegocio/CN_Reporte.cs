using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    /// <summary>
    /// Gestiona la generación de reportes resumidos de compras y ventas
    /// aplicando filtros por fechas, proveedor o tipo de comprobante.
    /// </summary>
    public class CN_Reporte
    {
        // Instancia de la capa de datos para ejecutar las consultas de reportes en SQL Server
        private CD_Reporte objcd_reporte = new CD_Reporte();

        /// <summary>
        /// Obtiene el listado resumido de compras filtradas por rango de fechas y proveedor.
        /// </summary>
        public List<ReporteCompra> CompraSinDetalle(string fechainicio, string fechafin, int idproveedor)
        {
            return objcd_reporte.CompraSinDetalle(fechainicio, fechafin, idproveedor);
        }

        /// <summary>
        /// Obtiene el listado resumido de ventas filtradas por rango de fechas y tipo de documento.
        /// </summary>
        public List<ReporteVenta> VentaSinDetalle(string fechainicio, string fechafin, string tipodocumento)
        {
            return objcd_reporte.VentaSinDetalle(fechainicio, fechafin, tipodocumento);
        }
    }
}
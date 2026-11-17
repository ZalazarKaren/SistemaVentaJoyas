using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    /// <summary>
    /// Gestiona la consulta de registros históricos y auditoría
    /// de ventas y compras realizadas en el sistema.
    /// </summary>
    public class CN_Historico
    {
        // Instancia de la capa de datos para consultar las tablas HISTORICO_VENTA e HISTORICO_COMPRA
        private CD_Historico objcd_historico = new CD_Historico();

        /// <summary>
        /// Obtiene el listado histórico de ventas filtradas dentro de un rango de fechas.
        /// </summary>
        public List<HistoricoVenta> ListarVentas(string fechaInicio, string fechaFin)
        {
            return objcd_historico.ListarVentas(fechaInicio, fechaFin);
        }

        /// <summary>
        /// Obtiene el listado histórico de compras filtradas dentro de un rango de fechas.
        /// </summary>
        public List<HistoricoCompra> ListarCompras(string fechaInicio, string fechaFin)
        {
            return objcd_historico.ListarCompras(fechaInicio, fechaFin);
        }
    }
}
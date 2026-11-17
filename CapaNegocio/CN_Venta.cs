using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    /// <summary>
    /// Gestiona las operaciones de negocio de ventas, incluyendo el control
    /// temporal de stock, el registro de comprobantes y la consulta de detalles.
    /// </summary>
    public class CN_Venta
    {
        // Instancia de la capa de datos para operar sobre VENTA, DETALLE_VENTA y PRODUCTO
        private CD_Venta objcd_venta = new CD_Venta();

        /// <summary>
        /// Descuenta unidades del stock de un producto al agregarlo al carrito de venta.
        /// </summary>
        public bool RestarStock(int idproducto, int cantidad)
        {
            return objcd_venta.RestarStock(idproducto, cantidad);
        }

        /// <summary>
        /// Restituye unidades al stock si se elimina un producto del carrito antes de confirmar la venta.
        /// </summary>
        public bool SumarStock(int idproducto, int cantidad)
        {
            return objcd_venta.SumarStock(idproducto, cantidad);
        }

        /// <summary>
        /// Obtiene el siguiente número correlativo disponible para emitir un comprobante de venta.
        /// </summary>
        public int ObtenerCorrelativo()
        {
            return objcd_venta.ObtenerCorrelativo();
        }

        /// <summary>
        /// Registra la cabecera de la venta junto con su tabla de productos vendidos.
        /// </summary>
        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            return objcd_venta.Registrar(obj, DetalleVenta, out Mensaje);
        }

        /// <summary>
        /// Busca una venta por su número de comprobante y carga su lista de artículos asociados.
        /// </summary>
        public Venta ObtenerVenta(string numero)
        {
            // 1. Recuperar los datos principales de la venta
            Venta oVenta = objcd_venta.ObtenerVenta(numero);

            // 2. Si el comprobante existe, obtener el detalle de productos vendidos
            if (oVenta.IdVenta != 0)
            {
                List<Detalle_Venta> oDetalleVenta = objcd_venta.ObtenerDetalleVenta(oVenta.IdVenta);
                oVenta.oDetalleVenta = oDetalleVenta;
            }

            return oVenta;
        }
    }
}
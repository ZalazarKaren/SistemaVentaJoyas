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
    /// Gestiona las operaciones de negocio relacionadas con el registro
    /// y la consulta de compras a proveedores.
    /// </summary>
    public class CN_Compra
    {
        // Instancia de la capa de datos para interactuar con las tablas COMPRA y DETALLE_COMPRA
        private CD_Compra objcd_compra = new CD_Compra();

        /// <summary>
        /// Obtiene el siguiente número correlativo disponible para generar un comprobante de compra.
        /// </summary>
        public int ObtenerCorrelativo()
        {
            return objcd_compra.ObtenerCorrelativo();
        }

        /// <summary>
        /// Registra una nueva compra junto con su tabla de detalle de productos.
        /// </summary>
        public bool Registrar(Compra obj, DataTable DetalleCompra, out string Mensaje)
        {
            return objcd_compra.Registrar(obj, DetalleCompra, out Mensaje);
        }

        /// <summary>
        /// Busca una compra por su número de documento y carga su lista de productos asociados.
        /// </summary>
        public Compra ObtenerCompra(string numero)
        {
            // 1. Obtener los datos de cabecera de la compra
            Compra oCompra = objcd_compra.ObtenerCompra(numero);

            // 2. Si la compra existe en la base de datos, recuperar sus ítems de detalle
            if (oCompra.IdCompra != 0)
            {
                List<Detalle_Compra> oDetalleCompra = objcd_compra.ObtenerDetalleCompra(oCompra.IdCompra);
                oCompra.oDetalleCompra = oDetalleCompra;
            }

            return oCompra;
        }
    }
}
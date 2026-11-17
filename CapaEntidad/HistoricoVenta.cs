using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class HistoricoVenta
    {
        public int IdHistoricoVenta { get; set; }
        public int IdVenta { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string DocumentoCliente { get; set; }
        public string NombreCliente { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioRegistro { get; set; }
        public decimal MontoPago { get; set; }
        public decimal MontoCambio { get; set; }
        public decimal MontoTotal { get; set; }
        public string Accion { get; set; }
        public string FechaHistorico { get; set; }
    }
}

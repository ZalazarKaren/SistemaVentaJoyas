using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class HistoricoCompra
    {
        public int IdHistoricoCompra { get; set; }
        public int IdCompra { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public int IdProveedor { get; set; }
        public string DocumentoProveedor { get; set; }
        public string RazonSocialProveedor { get; set; }
        public int IdUsuario { get; set; }
        public string UsuarioRegistro { get; set; }
        public decimal MontoTotal { get; set; }
        public string Accion { get; set; }
        public string FechaHistorico { get; set; }
    }
}

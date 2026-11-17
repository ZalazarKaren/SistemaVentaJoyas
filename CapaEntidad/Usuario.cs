using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Usuario : Persona
    {
        public int IdUsuario { get; set; }
        public string Clave { get; set; }
        public Rol oRol { get; set; }

    }
}

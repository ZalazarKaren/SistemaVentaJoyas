using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization; // Librería agregada para el formato de mayúsculas

namespace CapaNegocio
{
    public class CN_Cliente
    {
        private CD_Cliente objcd_Cliente = new CD_Cliente();

        public List<Cliente> Listar()
        {
            return objcd_Cliente.Listar();
        }

        public int Registrar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "El documento no puede estar vacío\n";
            }
            // Validaciones separadas para Nombre y Apellido
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "El nombre no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.Apellido))
            {
                Mensaje += "El apellido no puede estar vacío\n";
            }
            if (obj.Correo == "")
            {
                Mensaje += "El correo no puede estar vacío\n";
            }

            if (Mensaje != string.Empty)
            {
                return 0;
            }
            else
            {
                // Estandarización de Mayúsculas (Ej: convierte "mArIa goMez" en "Maria Gomez")
                TextInfo formatoTexto = CultureInfo.CurrentCulture.TextInfo;
                obj.Nombre = formatoTexto.ToTitleCase(obj.Nombre.ToLower());
                obj.Apellido = formatoTexto.ToTitleCase(obj.Apellido.ToLower());

                return objcd_Cliente.Registrar(obj, out Mensaje);
            }
        }

        public bool Editar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "El documento no puede estar vacío\n";
            }
            // Validaciones separadas para Nombre y Apellido
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "El nombre no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.Apellido))
            {
                Mensaje += "El apellido no puede estar vacío\n";
            }
            if (obj.Correo == "")
            {
                Mensaje += "El correo no puede estar vacío\n";
            }

            if (Mensaje != string.Empty)
            {
                return false;
            }
            else
            {
                // Estandarización de Mayúsculas
                TextInfo formatoTexto = CultureInfo.CurrentCulture.TextInfo;
                obj.Nombre = formatoTexto.ToTitleCase(obj.Nombre.ToLower());
                obj.Apellido = formatoTexto.ToTitleCase(obj.Apellido.ToLower());

                return objcd_Cliente.Editar(obj, out Mensaje);
            }
        }

        public bool Eliminar(Cliente obj, out string Mensaje)
        {
            return objcd_Cliente.Eliminar(obj, out Mensaje);
        }
    }
}
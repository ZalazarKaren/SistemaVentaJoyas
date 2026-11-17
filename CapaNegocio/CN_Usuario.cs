using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;
using CapaEntidad;
using System.Globalization; // Librería necesaria para el formato de mayúsculas

namespace CapaNegocio
{
    public class CN_Usuario
    {
        // Instancia para comunicarse con la capa de datos de usuarios
        private CD_Usuario objcd_usuario = new CD_Usuario();

        // Método para obtener la lista completa de usuarios
        public List<Usuario> Listar()
        {
            return objcd_usuario.Listar();
        }

        // Método para registrar un usuario aplicando reglas de validación previas
        public int Registrar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // Validaciones separadas para Documento, Nombre, Apellido y Clave
            if (obj.Documento == "")
            {
                Mensaje += "El documento no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "El nombre no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.Apellido))
            {
                Mensaje += "El apellido no puede estar vacío\n";
            }
            if (obj.Clave == "")
            {
                Mensaje += "La clave no puede estar vacía\n";
            }

            // Si se acumularon errores, se bloquea el registro y se retorna 0
            if (Mensaje != string.Empty)
            {
                return 0;
            }
            else
            {
                // Estandarización de Mayúsculas (Convierte "kArEn zalazar" a "Karen Zalazar")
                TextInfo formatoTexto = CultureInfo.CurrentCulture.TextInfo;
                obj.Nombre = formatoTexto.ToTitleCase(obj.Nombre.ToLower());
                obj.Apellido = formatoTexto.ToTitleCase(obj.Apellido.ToLower());

                // Si pasa las validaciones, continúa hacia la capa de datos
                return objcd_usuario.Registrar(obj, out Mensaje);
            }
        }

        // Método para editar un usuario existente aplicando reglas de validación previas
        public bool Editar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // Validaciones separadas
            if (obj.Documento == "")
            {
                Mensaje += "El documento no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje += "El nombre no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.Apellido))
            {
                Mensaje += "El apellido no puede estar vacío\n";
            }
            if (obj.Clave == "")
            {
                Mensaje += "La clave no puede estar vacía\n";
            }

            // Si se acumularon errores, se bloquea la edición y se retorna false
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

                // Si pasa las validaciones, continúa hacia la capa de datos
                return objcd_usuario.Editar(obj, out Mensaje);
            }
        }

        // Método para eliminar un usuario a través de la capa de datos
        public bool Eliminar(Usuario obj, out string Mensaje)
        {
            return objcd_usuario.Eliminar(obj, out Mensaje);
        }
    }
}
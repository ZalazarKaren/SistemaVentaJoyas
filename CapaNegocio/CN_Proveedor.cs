using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization; // Librería para el formato de mayúsculas

namespace CapaNegocio
{
    public class CN_Proveedor
    {
        private CD_Proveedor objcd_Proveedor = new CD_Proveedor();

        public List<Proveedor> Listar()
        {
            return objcd_Proveedor.Listar();
        }

        public int Registrar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "El documento no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.RazonSocial))
            {
                Mensaje += "La razón social no puede estar vacía\n";
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
                // Estandarización de Mayúsculas para la Razón Social
                TextInfo formatoTexto = CultureInfo.CurrentCulture.TextInfo;
                obj.RazonSocial = formatoTexto.ToTitleCase(obj.RazonSocial.ToLower());

                return objcd_Proveedor.Registrar(obj, out Mensaje);
            }
        }

        public bool Editar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.Documento == "")
            {
                Mensaje += "El documento no puede estar vacío\n";
            }
            if (string.IsNullOrWhiteSpace(obj.RazonSocial))
            {
                Mensaje += "La razón social no puede estar vacía\n";
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
                // Estandarización de Mayúsculas para la Razón Social
                TextInfo formatoTexto = CultureInfo.CurrentCulture.TextInfo;
                obj.RazonSocial = formatoTexto.ToTitleCase(obj.RazonSocial.ToLower());

                return objcd_Proveedor.Editar(obj, out Mensaje);
            }
        }

        public bool Eliminar(Proveedor obj, out string Mensaje)
        {
            return objcd_Proveedor.Eliminar(obj, out Mensaje);
        }

        // ====================================================================
        // NUEVOS MÉTODOS PARA LA TABLA INTERMEDIA (PUNTO 1: PROVEEDOR-CATEGORIA)
        // ====================================================================

        public bool AsignarCategoria(int IdProveedor, int IdCategoria, out string Mensaje)
        {
            return objcd_Proveedor.AsignarCategoria(IdProveedor, IdCategoria, out Mensaje);
        }

        public bool QuitarCategoria(int IdProveedor, int IdCategoria, out string Mensaje)
        {
            return objcd_Proveedor.QuitarCategoria(IdProveedor, IdCategoria, out Mensaje);
        }

        public List<Categoria> ListarCategorias(int IdProveedor)
        {
            return objcd_Proveedor.ListarCategorias(IdProveedor);
        }
    }
}
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    /// <summary>
    /// Gestiona el acceso a datos en SQL Server para consultar los roles
    /// de usuario disponibles en el sistema.
    /// </summary>
    public class CD_Rol
    {
        /// <summary>
        /// Obtiene la lista completa de roles registrados en la tabla ROL.
        /// </summary>
        public List<Rol> Listar()
        {
            List<Rol> lista = new List<Rol>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // 1. Definir la consulta para recuperar el identificador y descripción de cada rol
                    StringBuilder query = new StringBuilder();
                    query.AppendLine("select IdRol, Descripcion from ROL");

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    // 2. Leer los registros devueltos y mapear cada fila a un objeto Rol
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Rol()
                            {
                                IdRol = Convert.ToInt32(dr["IdRol"]),
                                Descripcion = dr["Descripcion"].ToString()
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // Si ocurre un error de conexión o lectura, devolver una lista vacía
                    lista = new List<Rol>();
                }
            }

            return lista;
        }
    }
}
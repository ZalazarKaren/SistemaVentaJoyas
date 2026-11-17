using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_Usuario
    {
        // --- 1. MÉTODO LISTAR ---
        // Se encarga de ir a la base de datos, buscar todos los usuarios activos y devolverlos en una Lista para llenar la grilla.
        public List<Usuario> Listar()
        {
            List<Usuario> lista = new List<Usuario>();

            // 'using' asegura que la conexión a la base de datos se cierre automáticamente al terminar, liberando memoria.
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    StringBuilder query = new StringBuilder();
                    // Armamos la consulta SQL uniendo 3 tablas: USUARIO, PERSONA (para traer nombre/apellido) y ROL (para el tipo de acceso)
                    query.AppendLine("SELECT u.IdUsuario, p.Documento, p.Nombre, p.Apellido, p.Correo, u.Clave, p.Estado, r.IdRol, r.Descripcion ");
                    query.AppendLine("FROM USUARIO u ");
                    query.AppendLine("INNER JOIN PERSONA p ON u.IdPersona = p.IdPersona ");
                    query.AppendLine("INNER JOIN ROL r ON r.IdRol = u.IdRol ");
                    query.AppendLine("WHERE p.Estado = 1"); // Solo traemos los usuarios que no han sido dados de baja (Baja Lógica)

                    SqlCommand cmd = new SqlCommand(query.ToString(), oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open(); // Abrimos la conexión con SQL Server

                    // SqlDataReader lee los resultados de la consulta fila por fila
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // Por cada fila que encuentra en SQL, crea un nuevo objeto Usuario y lo mete en la lista
                            lista.Add(new Usuario()
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Documento = dr["Documento"].ToString(),
                                Nombre = dr["Nombre"].ToString(),     // Dato separado según la 1ra Forma Normal
                                Apellido = dr["Apellido"].ToString(), // Dato separado según la 1ra Forma Normal
                                Correo = dr["Correo"].ToString(),
                                Clave = dr["Clave"].ToString(),
                                Estado = Convert.ToBoolean(dr["Estado"]),
                                oRol = new Rol()
                                {
                                    IdRol = Convert.ToInt32(dr["IdRol"]),
                                    Descripcion = dr["Descripcion"].ToString()
                                }
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // Si ocurre un error (ej: base de datos caída), devuelve la lista vacía para que el programa no explote
                    lista = new List<Usuario>();
                }
            }
            return lista;
        }

        // --- 2. MÉTODO REGISTRAR ---
        // Llama a un Procedimiento Almacenado en SQL para insertar un usuario. Devuelve el ID generado si sale bien, o 0 si falla.
        public int Registrar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            int idusuariogenerado = 0;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    // Llamamos al Procedimiento Almacenado que debe encargarse de hacer los INSERT en PERSONA y en USUARIO
                    SqlCommand cmd = new SqlCommand("SP_REGISTRARUSUARIO", oconexion);
                    cmd.Parameters.AddWithValue("Documento", obj.Documento);
                    cmd.Parameters.AddWithValue("Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("Apellido", obj.Apellido);
                    cmd.Parameters.AddWithValue("Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("Clave", obj.Clave);
                    cmd.Parameters.AddWithValue("IdRol", obj.oRol.IdRol);
                    cmd.Parameters.AddWithValue("Estado", obj.Estado);

                    // Parámetros de salida (Output): SQL nos devuelve variables por acá (el ID nuevo y un mensaje si hubo error)
                    cmd.Parameters.Add("IdUsuarioResultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery(); // Ejecutamos el procedimiento

                    // Capturamos las respuestas que nos devolvió SQL
                    idusuariogenerado = Convert.ToInt32(cmd.Parameters["IdUsuarioResultado"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                idusuariogenerado = 0;
                Mensaje = ex.Message; // Captura el error de C# (ej: mala conexión)
            }

            return idusuariogenerado;
        }

        // --- 3. MÉTODO EDITAR ---
        // Llama a un Procedimiento Almacenado para actualizar los datos. Devuelve true si fue exitoso, false si falló.
        public bool Editar(Usuario obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("SP_EDITARUSUARIO", oconexion);
                    cmd.Parameters.AddWithValue("IdUsuario", obj.IdUsuario);
                    cmd.Parameters.AddWithValue("Documento", obj.Documento);
                    cmd.Parameters.AddWithValue("Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("Apellido", obj.Apellido);
                    cmd.Parameters.AddWithValue("Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("Clave", obj.Clave);
                    cmd.Parameters.AddWithValue("IdRol", obj.oRol.IdRol);
                    cmd.Parameters.AddWithValue("Estado", obj.Estado);

                    cmd.Parameters.Add("Respuesta", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Respuesta"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                respuesta = false;
                Mensaje = ex.Message;
            }

            return respuesta;
        }

        // --- 4. MÉTODO ELIMINAR ---
        // Hace una baja lógica (cambia el Estado a 0) o lo borra de la BD, dependiendo de cómo esté armado el Procedimiento en SQL.
        public bool Eliminar(Usuario obj, out string Mensaje)
        {
            bool respuesta = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("SP_ELIMINARUSUARIO", oconexion);
                    cmd.Parameters.AddWithValue("IdUsuario", obj.IdUsuario);

                    cmd.Parameters.Add("Respuesta", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Respuesta"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                respuesta = false;
                Mensaje = ex.Message;
            }

            return respuesta;
        }
    }
}
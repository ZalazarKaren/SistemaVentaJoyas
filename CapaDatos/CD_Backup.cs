using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace CapaDatos
{
    /// <summary>
    /// Ejecuta directamente en SQL Server la validación de credenciales,
    /// la generación de respaldos (.bak) y la restauración de la base de datos.
    /// </summary>
    public class CD_Backup
    {
        /// <summary>
        /// Verifica en la tabla USUARIO si la clave ingresada coincide con el usuario activo.
        /// </summary>
        public bool ValidarClave(int idUsuario, string clave)
        {
            bool esValida = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // Consulta parametrizada para evitar inyección SQL
                    string query = "SELECT COUNT(1) FROM dbo.USUARIO WHERE IdUsuario = @idusuario AND Clave = @clave";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@idusuario", idUsuario);
                    cmd.Parameters.AddWithValue("@clave", clave);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    // ExecuteScalar devuelve el conteo; si es mayor a 0, la clave es correcta
                    esValida = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch
                {
                    esValida = false;
                }
            }

            return esValida;
        }

        /// <summary>
        /// Crea un archivo .bak completo de la base de datos configurada en la cadena de conexión.
        /// </summary>
        public bool GenerarBackup(string rutaArchivo, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // 1. Obtener dinámicamente el nombre de la base de datos desde Conexion.cadena
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(Conexion.cadena);
                string nombreBD = builder.InitialCatalog;

                // 2. Crear la carpeta de destino en disco si todavía no existe
                string carpeta = Path.GetDirectoryName(rutaArchivo);
                if (!string.IsNullOrEmpty(carpeta) && !Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                // 3. Ejecutar el respaldo en SQL Server (WITH FORMAT, INIT sobrescribe medios previos si existieran)
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    string query = $"BACKUP DATABASE [{nombreBD}] TO DISK = @ruta WITH FORMAT, INIT, NAME = @nombre";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@ruta", rutaArchivo);
                    cmd.Parameters.AddWithValue("@nombre", $"Respaldo {nombreBD} {DateTime.Now:dd/MM/yyyy HH:mm}");
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 120; // Tiempo de espera ampliado (2 min) para bases grandes

                    oconexion.Open();
                    cmd.ExecuteNonQuery();
                }

                mensaje = $"Copia de seguridad generada correctamente en:\n{rutaArchivo}";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error al generar la copia de seguridad: " + ex.Message +
                          "\n\nSugerencia: Guardá el respaldo en una carpeta directa en C:\\ (por ejemplo C:\\Respaldos_Joyeria) para que SQL Server tenga permisos de escritura.";
                return false;
            }
        }

        /// <summary>
        /// Restaura la base de datos desde un archivo .bak reemplazando su estado actual.
        /// </summary>
        public bool RestaurarBackup(string rutaArchivo, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                // 1. Guardar el nombre de la BD destino y apuntar la conexión a 'master'
                // (SQL Server no permite restaurar una base mientras hay conexiones activas sobre ella)
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(Conexion.cadena);
                string nombreBD = builder.InitialCatalog;
                builder.InitialCatalog = "master";

                using (SqlConnection oconexion = new SqlConnection(builder.ConnectionString))
                {
                    oconexion.Open();

                    // 2. Pasar la BD a modo monousuario y cerrar conexiones abiertas de inmediato
                    string querySingleUser = $"ALTER DATABASE [{nombreBD}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    using (SqlCommand cmdSingle = new SqlCommand(querySingleUser, oconexion))
                    {
                        cmdSingle.ExecuteNonQuery();
                    }

                    try
                    {
                        // 3. Ejecutar la restauración reemplazando la base existente (WITH REPLACE)
                        string queryRestore = $"RESTORE DATABASE [{nombreBD}] FROM DISK = @ruta WITH REPLACE;";
                        using (SqlCommand cmdRestore = new SqlCommand(queryRestore, oconexion))
                        {
                            cmdRestore.Parameters.AddWithValue("@ruta", rutaArchivo);
                            cmdRestore.CommandTimeout = 180; // Tiempo de espera ampliado (3 min)
                            cmdRestore.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        // 4. Devolver siempre la BD a modo multiusuario, incluso si la restauración falla
                        string queryMultiUser = $"ALTER DATABASE [{nombreBD}] SET MULTI_USER;";
                        using (SqlCommand cmdMulti = new SqlCommand(queryMultiUser, oconexion))
                        {
                            cmdMulti.ExecuteNonQuery();
                        }
                    }
                }

                mensaje = "La base de datos fue restaurada correctamente.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error al restaurar la copia de seguridad: " + ex.Message;
                return false;
            }
        }
    }
}
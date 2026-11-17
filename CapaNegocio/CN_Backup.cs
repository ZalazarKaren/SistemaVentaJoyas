using CapaDatos;
using System;
using System.IO;

namespace CapaNegocio
{
    /// <summary>
    /// Gestiona las reglas de negocio y validaciones de seguridad
    /// para respaldar y restaurar la base de datos.
    /// </summary>
    public class CN_Backup
    {
        // Instancia de la capa de datos para ejecutar las operaciones en SQL Server
        private CD_Backup objcd_backup = new CD_Backup();

        /// <summary>
        /// Valida credenciales y genera un archivo de respaldo (.bak) con marca de tiempo.
        /// </summary>
        public bool GenerarBackup(int idUsuario, string clave, string carpetaDestino, out string mensaje)
        {
            mensaje = string.Empty;

            // 1. Validar entradas obligatorias antes de abrir conexión con la BD
            if (string.IsNullOrWhiteSpace(clave))
            {
                mensaje = "Debe ingresar su contraseña para autorizar el respaldo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(carpetaDestino))
            {
                mensaje = "Debe seleccionar una carpeta de destino.";
                return false;
            }

            // 2. Confirmar la identidad del usuario que autoriza la operación
            if (!objcd_backup.ValidarClave(idUsuario, clave))
            {
                mensaje = "La contraseña ingresada es incorrecta.";
                return false;
            }

            // 3. Armar nombre único con fecha y hora para evitar sobrescribir respaldos anteriores
            string nombreArchivo = $"Respaldo_SistemaJoyas_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            // 4. Delegar la ejecución del comando BACKUP DATABASE a CapaDatos
            return objcd_backup.GenerarBackup(rutaCompleta, out mensaje);
        }

        /// <summary>
        /// Valida credenciales y existencia del archivo .bak antes de restaurar la base de datos.
        /// </summary>
        public bool RestaurarBackup(int idUsuario, string clave, string rutaArchivo, out string mensaje)
        {
            mensaje = string.Empty;

            // 1. Validar que se haya ingresado la clave de autorización
            if (string.IsNullOrWhiteSpace(clave))
            {
                mensaje = "Debe ingresar su contraseña para autorizar la restauración.";
                return false;
            }

            // 2. Verificar que la ruta no esté vacía y que el archivo exista físicamente en disco
            if (string.IsNullOrWhiteSpace(rutaArchivo) || !File.Exists(rutaArchivo))
            {
                mensaje = "Debe seleccionar un archivo .bak válido.";
                return false;
            }

            // 3. Confirmar credenciales del usuario antes de sobrescribir la base de datos
            if (!objcd_backup.ValidarClave(idUsuario, clave))
            {
                mensaje = "La contraseña ingresada es incorrecta.";
                return false;
            }

            // 4. Delegar la ejecución del comando RESTORE DATABASE a CapaDatos
            return objcd_backup.RestaurarBackup(rutaArchivo, out mensaje);
        }
    }
}
using CapaEntidad;
using CapaNegocio;
using System;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario de interfaz de usuario para generar y restaurar
    /// copias de seguridad (.bak) de la base de datos.
    /// </summary>
    public partial class frmBackup : Form
    {
        // Almacena los datos del usuario que inició sesión para validar su identidad
        private Usuario usuarioActual;

        /// <summary>
        /// Constructor del formulario. Si no recibe un usuario activo, asigna uno por defecto.
        /// </summary>
        public frmBackup(Usuario objusuario = null)
        {
            usuarioActual = objusuario ?? new Usuario() { IdUsuario = 1, Nombre = "ADMIN", Apellido = "PREDEFINIDO" };
            InitializeComponent();
        }

        /// <summary>
        /// Muestra el nombre del operador activo y enfoca el campo de contraseña al abrir.
        /// </summary>
        private void frmBackup_Load(object sender, EventArgs e)
        {
            lblUsuarioInfo.Text = $"Usuario activo: {usuarioActual.Nombre} {usuarioActual.Apellido}";
            txtclave.Focus();
        }

        /// <summary>
        /// Alterna entre ocultar o mostrar los caracteres de la contraseña ingresada.
        /// </summary>
        private void chkverclave_CheckedChanged(object sender, EventArgs e)
        {
            txtclave.UseSystemPasswordChar = !chkverclave.Checked;
        }

        /// <summary>
        /// Abre el explorador de carpetas para elegir dónde se guardará el respaldo.
        /// </summary>
        private void btnexaminarcarpeta_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccione la carpeta donde se guardará la copia de seguridad";
                folderDialog.SelectedPath = txtrutacarpeta.Text;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtrutacarpeta.Text = folderDialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Solicita a la capa de negocio la generación del archivo .bak y muestra el resultado.
        /// </summary>
        private void btngenerarbackup_Click(object sender, EventArgs e)
        {
            string mensaje = string.Empty;

            // 1. Cambiar el cursor a espera mientras SQL Server procesa el respaldo
            Cursor = Cursors.WaitCursor;
            bool resultado = new CN_Backup().GenerarBackup(
                usuarioActual.IdUsuario,
                txtclave.Text,
                txtrutacarpeta.Text,
                out mensaje
            );
            Cursor = Cursors.Default;

            // 2. Limpiar la clave por seguridad si tuvo éxito y notificar al usuario
            if (resultado)
            {
                txtclave.Text = "";
                MessageBox.Show(mensaje, "Respaldo Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(mensaje, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Abre el selector de archivos filtrado únicamente para respaldos SQL (*.bak).
        /// </summary>
        private void btnexaminararchivo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Title = "Seleccione el archivo de respaldo (.bak)";
                openDialog.Filter = "Archivos de Respaldo SQL (*.bak)|*.bak";
                openDialog.InitialDirectory = txtrutacarpeta.Text;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    txtrutaarchivo.Text = openDialog.FileName;
                }
            }
        }

        /// <summary>
        /// Solicita confirmación al usuario y ejecuta la restauración de la base de datos.
        /// </summary>
        private void btnrestaurarbackup_Click(object sender, EventArgs e)
        {
            // 1. Verificar que se haya seleccionado un archivo antes de pedir confirmación
            if (string.IsNullOrWhiteSpace(txtrutaarchivo.Text))
            {
                MessageBox.Show("Seleccione primero un archivo .bak para restaurar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Advertir al usuario que la restauración sobrescribirá los datos actuales
            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de restaurar la base de datos? Se sobrescribirán todos los movimientos posteriores al respaldo.",
                "Confirmar Restauración",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacion != DialogResult.Yes) return;

            string mensaje = string.Empty;

            // 3. Ejecutar la restauración mostrando el cursor de espera
            Cursor = Cursors.WaitCursor;
            bool resultado = new CN_Backup().RestaurarBackup(
                usuarioActual.IdUsuario,
                txtclave.Text,
                txtrutaarchivo.Text,
                out mensaje
            );
            Cursor = Cursors.Default;

            // 4. Informar el desenlace de la operación
            if (resultado)
            {
                txtclave.Text = "";
                MessageBox.Show(mensaje, "Restauración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
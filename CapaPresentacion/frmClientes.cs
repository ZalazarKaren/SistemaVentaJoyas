using CapaEntidad;
using CapaNegocio;
using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario para la gestión integral (ABM) de clientes, incluyendo validaciones
    /// de entrada, estandarización de nombres y filtrado dinámico en grilla.
    /// </summary>
    public partial class frmClientes : Form
    {
        private Image imagenCheck;
        private Image imagenDelete;

        /// <summary>
        /// Inicializa el formulario y carga los íconos visuales utilizados en la grilla.
        /// </summary>
        public frmClientes()
        {
            InitializeComponent();

            // Buscar dinámicamente la ruta de la carpeta "Images" en el proyecto
            string carpetaImagenes = Path.Combine(
                Directory.GetParent(Application.StartupPath).Parent.Parent.FullName,
                "CapaPresentacion",
                "Images"
            );

            imagenCheck = Image.FromFile(Path.Combine(carpetaImagenes, "check20.png"));
            imagenDelete = Image.FromFile(Path.Combine(carpetaImagenes, "delete20.png"));
        }

        /// <summary>
        /// Configura las validaciones de teclado, carga los combos de estado y búsqueda,
        /// y pobla la grilla con los clientes registrados en la base de datos.
        /// </summary>
        private void frmClientes_Load(object sender, EventArgs e)
        {
            // Vincular el evento KeyPress para que Documento y Teléfono solo acepten números
            txtdocumento.KeyPress += new KeyPressEventHandler(ValidarNumeros_KeyPress);
            txttelefono.KeyPress += new KeyPressEventHandler(ValidarNumeros_KeyPress);

            // Llenar el desplegable de Estados (1 = Activo, 0 = No Activo)
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = " No Activo" });
            cboestado.DisplayMember = "Texto";
            cboestado.ValueMember = "Valor";
            cboestado.SelectedIndex = 0;

            // Llenar el buscador dinámicamente leyendo las columnas visibles de la grilla
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                if (columna.Visible == true && columna.Name != "btnseleccionar")
                {
                    cbobusqueda.Items.Add(new OpcionCombo() { Valor = columna.Name, Texto = columna.HeaderText });
                }
            }
            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Carga inicial de clientes desde la capa de negocio
            List<Cliente> lista = new CN_Cliente().Listar();

            foreach (Cliente item in lista)
            {
                // Unir Apellido y Nombre con coma para una visualización uniforme en la grilla
                string apellidoYNombre = item.Apellido + ", " + item.Nombre;

                dgvdata.Rows.Add(new object[] {
                    "",
                    item.IdCliente,
                    item.Documento,
                    apellidoYNombre,
                    item.Correo,
                    item.Telefono,
                    item.Estado == true ? 1 : 0,
                    item.Estado == true ? "Activo" : "No Activo"
                });
            }
        }

        /// <summary>
        /// Bloquea cualquier tecla que no sea un dígito numérico o una tecla de control.
        /// </summary>
        private void ValidarNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Estandariza una cadena de texto aplicando formato Título (iniciales en mayúscula).
        /// </summary>
        private string FormatearTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(texto.Trim().ToLower());
        }

        /// <summary>
        /// Verifica mediante expresión regular que el correo electrónico tenga un formato válido.
        /// </summary>
        private bool EsCorreoValido(string correo)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex regex = new Regex(patron);
            return regex.IsMatch(correo);
        }

        /// <summary>
        /// Valida los campos ingresados y registra un nuevo cliente o actualiza uno existente
        /// según el identificador cargado en el formulario.
        /// </summary>
        private void btnguardar_Click(object sender, EventArgs e)
        {
            // 1. Validar que no falten los datos obligatorios
            if (string.IsNullOrWhiteSpace(txtdocumento.Text) ||
                string.IsNullOrWhiteSpace(txtnombre.Text) ||
                string.IsNullOrWhiteSpace(txtapellido.Text))
            {
                MessageBox.Show("Debe completar los campos obligatorios (Documento, Nombre y Apellido).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtdocumento.Focus();
                return;
            }

            // 2. Validar el formato del correo electrónico si fue ingresado
            if (!string.IsNullOrWhiteSpace(txtcorreo.Text) && !EsCorreoValido(txtcorreo.Text))
            {
                MessageBox.Show("El formato del correo electrónico no es válido.\nEjemplo: cliente@dominio.com", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtcorreo.Focus();
                return;
            }

            string mensaje = string.Empty;

            // 3. Construir la entidad Cliente aplicando estandarización de texto
            Cliente obj = new Cliente()
            {
                IdCliente = Convert.ToInt32(txtid.Text == "" ? "0" : txtid.Text),
                Documento = txtdocumento.Text,
                Nombre = FormatearTexto(txtnombre.Text),
                Apellido = FormatearTexto(txtapellido.Text),
                Correo = txtcorreo.Text.Trim().ToLower(),
                Telefono = txttelefono.Text,
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false
            };

            // 4. Alta de cliente nuevo (IdCliente == 0) o edición de cliente existente
            if (obj.IdCliente == 0)
            {
                int idegenerado = new CN_Cliente().Registrar(obj, out mensaje);

                if (idegenerado != 0)
                {
                    string apellidoYNombre = obj.Apellido + ", " + obj.Nombre;

                    dgvdata.Rows.Add(new object[] {
                        "",
                        idegenerado,
                        txtdocumento.Text,
                        apellidoYNombre,
                        txtcorreo.Text,
                        txttelefono.Text,
                        ((OpcionCombo)cboestado.SelectedItem).Valor.ToString(),
                        ((OpcionCombo)cboestado.SelectedItem).Texto.ToString()
                    });
                    Limpiar();
                }
                else
                {
                    MessageBox.Show(mensaje);
                }
            }
            else
            {
                bool resultado = new CN_Cliente().Editar(obj, out mensaje);
                if (resultado)
                {
                    string apellidoYNombre = obj.Apellido + ", " + obj.Nombre;

                    DataGridViewRow row = dgvdata.Rows[Convert.ToInt32(txtindice.Text)];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["Documento"].Value = txtdocumento.Text;
                    row.Cells["NombreCompleto"].Value = apellidoYNombre;
                    row.Cells["Correo"].Value = txtcorreo.Text;
                    row.Cells["Telefono"].Value = txttelefono.Text;
                    row.Cells["EstadoValor"].Value = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString();
                    row.Cells["Estado"].Value = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString();

                    Limpiar();
                }
                else
                {
                    MessageBox.Show(mensaje);
                }
            }
        }

        /// <summary>
        /// Solicita confirmación y elimina de la base de datos y la grilla el cliente seleccionado.
        /// </summary>
        private void btneliminar_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtid.Text) != 0)
            {
                if (MessageBox.Show("¿Desea eliminar el cliente seleccionado?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string mensaje = string.Empty;
                    Cliente obj = new Cliente() { IdCliente = Convert.ToInt32(txtid.Text) };

                    bool respuesta = new CN_Cliente().Eliminar(obj, out mensaje);
                    if (respuesta)
                    {
                        dgvdata.Rows.RemoveAt(Convert.ToInt32(txtindice.Text));
                        Limpiar();
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        /// <summary>
        /// Dibuja el ícono de selección centrado en la primera columna de cada fila.
        /// </summary>
        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                int ancho = 20, alto = 20;
                int x = e.CellBounds.Left + (e.CellBounds.Width - ancho) / 2;
                int y = e.CellBounds.Top + (e.CellBounds.Height - alto) / 2;
                e.Graphics.DrawImage(imagenCheck, new Rectangle(x, y, ancho, alto));
                e.Handled = true;
            }
        }

        /// <summary>
        /// Transfiere los datos del cliente seleccionado en la grilla hacia los campos de edición.
        /// </summary>
        private void dgvdata_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgvdata.Columns["btnseleccionar"].Index)
            {
                int indice = e.RowIndex;
                txtindice.Text = indice.ToString();
                txtid.Text = dgvdata.Rows[indice].Cells["Id"].Value.ToString();
                txtdocumento.Text = dgvdata.Rows[indice].Cells["Documento"].Value.ToString();

                // Separar "Apellido, Nombre" para cargar cada campo por separado
                string nombreCompletoGrilla = dgvdata.Rows[indice].Cells["NombreCompleto"].Value.ToString();
                string[] partes = nombreCompletoGrilla.Split(',');

                if (partes.Length == 2)
                {
                    txtapellido.Text = partes[0].Trim();
                    txtnombre.Text = partes[1].Trim();
                }
                else
                {
                    txtnombre.Text = nombreCompletoGrilla;
                    txtapellido.Text = "";
                }

                txtcorreo.Text = dgvdata.Rows[indice].Cells["Correo"].Value.ToString();
                txttelefono.Text = dgvdata.Rows[indice].Cells["Telefono"].Value.ToString();

                // Seleccionar el estado correspondiente en el desplegable
                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(dgvdata.Rows[indice].Cells["EstadoValor"].Value))
                    {
                        cboestado.SelectedIndex = cboestado.Items.IndexOf(oc);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Filtra las filas de la grilla según la columna elegida y el texto de búsqueda ingresado.
        /// </summary>
        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Cells[columnaFiltro].Value.ToString().Trim().ToUpper().Contains(txtbusqueda.Text.Trim().ToUpper()))
                        row.Visible = true;
                    else
                        row.Visible = false;
                }
            }
        }

        /// <summary>
        /// Limpia el cuadro de búsqueda y vuelve a mostrar todas las filas de la grilla.
        /// </summary>
        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
        }

        /// <summary>
        /// Restablece todos los controles de entrada del formulario a su estado inicial.
        /// </summary>
        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtdocumento.Text = "";
            txtnombre.Text = "";
            txtapellido.Text = "";
            txtcorreo.Text = "";
            txttelefono.Text = "";
            cboestado.SelectedIndex = 0;
            txtdocumento.Select();
        }

        /// <summary>
        /// Limpia los campos de edición para preparar el alta de un nuevo registro.
        /// </summary>
        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        // Eventos vacíos generados por el diseñador de Windows Forms
        private void label1_Click(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void cboestado_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void txttelefono_TextChanged(object sender, EventArgs e) { }
        private void txtcorreo_TextChanged(object sender, EventArgs e) { }
    }
}
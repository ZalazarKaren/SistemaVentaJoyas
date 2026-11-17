using CapaEntidad;
using CapaNegocio;
using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Modales
{
    public partial class mdCliente : Form
    {
        // Propiedad para enviar el cliente seleccionado de vuelta a frmVentas
        public Cliente _Cliente { get; set; }

        public mdCliente()
        {
            InitializeComponent();
        }

        private void mdCliente_Load(object sender, EventArgs e)
        {
            // Llenar el ComboBox de búsqueda con las columnas visibles de la grilla
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                if (columna.Visible == true)
                {
                    cbobusqueda.Items.Add(new OpcionCombo() { Valor = columna.Name, Texto = columna.HeaderText });
                }
            }

            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Cargar los clientes desde la base de datos
            List<Cliente> lista = new CN_Cliente().Listar();

            foreach (Cliente item in lista)
            {
                if (item.Estado)
                {
                    // Concatenamos el Nombre y Apellido que vienen separados desde tu BD
                    string nombreCompleto = item.Nombre + " " + item.Apellido;

                    // Creamos una fila nueva vacía y asignamos por el nombre exacto de la columna 
                    // para evitar que los datos se desplacen a columnas incorrectas
                    int indiceFila = dgvdata.Rows.Add();
                    dgvdata.Rows[indiceFila].Cells["Documento"].Value = item.Documento;
                    dgvdata.Rows[indiceFila].Cells["NombreCompleto"].Value = nombreCompleto;
                }
            }
        }

        private void dgvdata_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int iRow = e.RowIndex;
            int iColum = e.ColumnIndex;

            if (iRow >= 0 && iColum >= 0)
            {
                // Usamos Convert.ToString para evitar el error NullReferenceException si la celda está vacía
                string nombreCompletoGrilla = Convert.ToString(dgvdata.Rows[iRow].Cells["NombreCompleto"].Value);

                // Separamos nuevamente para armar el objeto Cliente con las propiedades correctas
                string[] partes = nombreCompletoGrilla.Split(' ');
                string nombreExtraido = partes.Length > 0 ? partes[0] : "";
                string apellidoExtraido = partes.Length > 1 ? partes[1] : "";

                // Instanciamos y cargamos el cliente seleccionado
                _Cliente = new Cliente()
                {
                    Documento = Convert.ToString(dgvdata.Rows[iRow].Cells["Documento"].Value),
                    Nombre = nombreExtraido,
                    Apellido = apellidoExtraido
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    // Filtramos ignorando mayúsculas/minúsculas y espacios
                    if (row.Cells[columnaFiltro].Value.ToString().Trim().ToUpper().Contains(txtbusqueda.Text.Trim().ToUpper()))
                        row.Visible = true;
                    else
                        row.Visible = false;
                }
            }
        }

        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
        }
    }
}
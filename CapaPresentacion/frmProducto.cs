using CapaEntidad;
using CapaNegocio;
using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace CapaPresentacion
{
    public partial class frmProducto : Form
    {
        private Image imagenCheck;
        private Image imagenDelete;

        public frmProducto()
        {
            InitializeComponent();

            string carpetaImagenes = Path.Combine(
                 Directory.GetParent(Application.StartupPath).Parent.Parent.FullName,
                 "CapaPresentacion",
                 "Images"
            );

            imagenCheck = Image.FromFile(
                Path.Combine(carpetaImagenes, "check20.png")
            );

            imagenDelete = Image.FromFile(
                Path.Combine(carpetaImagenes, "delete20.png")
            );
        }

        private void frmProducto_Load(object sender, EventArgs e)
        {
            // Vinculamos la validación numérica al campo código al cargar el formulario
            txtcodigo.KeyPress += new KeyPressEventHandler(ValidarNumeros_KeyPress);

            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = " No Activo" });
            cboestado.DisplayMember = "Texto";
            cboestado.ValueMember = "Valor";
            cboestado.SelectedIndex = 0;

            List<Categoria> listacategoria = new CN_Categoria().Listar();

            foreach (Categoria item in listacategoria)
            {
                cbocategoria.Items.Add(new OpcionCombo() { Valor = item.IdCategoria, Texto = item.Descripcion });
            }
            cbocategoria.DisplayMember = "Texto";
            cbocategoria.ValueMember = "Valor";
            cbocategoria.SelectedIndex = 0;

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

            // Mostrar todos los Productos
            List<Producto> lista = new CN_Producto().Listar();

            foreach (Producto item in lista)
            {
                dgvdata.Rows.Add(new object[]
                {
                    "",
                    item.IdProducto,
                    item.Codigo,
                    item.Nombre,
                    item.Descripcion,
                    item.oCategoria.IdCategoria,
                    item.oCategoria.Descripcion,
                    item.Stock,
                    item.PrecioCompra.ToString("0.00"),
                    item.PrecioVenta.ToString("0.00"),
                    item.Estado == true ? 1 : 0,
                    item.Estado == true ? "Activo" : "No Activo"
                });
            }
        }

        // --- VALIDACIÓN VISUAL: Bloquear letras en campos numéricos ---
        private void ValidarNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            // --- VALIDACIONES VISUALES ANTES DE ENVIAR A LA CAPA DE NEGOCIO ---
            if (string.IsNullOrWhiteSpace(txtcodigo.Text) ||
                string.IsNullOrWhiteSpace(txtnombre.Text))
            {
                MessageBox.Show("Debe completar los campos obligatorios (Código y Nombre).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtcodigo.Focus();
                return; // Corta la ejecución si falta información esencial
            }
            // --- FIN VALIDACIONES ---

            string mensaje = string.Empty;

            Producto obj = new Producto()
            {
                IdProducto = Convert.ToInt32(txtid.Text == "" ? "0" : txtid.Text),
                Codigo = txtcodigo.Text,
                Nombre = txtnombre.Text,
                Descripcion = txtdescripcion.Text,
                oCategoria = new Categoria() { IdCategoria = Convert.ToInt32(((OpcionCombo)cbocategoria.SelectedItem).Valor) },
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false
            };

            if (obj.IdProducto == 0)
            {
                int idgenerado = new CN_Producto().Registrar(obj, out mensaje);

                if (idgenerado != 0)
                {
                    dgvdata.Rows.Add(new object[] {
                        "",
                        idgenerado,
                        txtcodigo.Text,
                        txtnombre.Text,
                        txtdescripcion.Text,
                        ((OpcionCombo)cbocategoria.SelectedItem).Valor.ToString(),
                        ((OpcionCombo)cbocategoria.SelectedItem).Texto.ToString(),
                        "0",
                        "0.00",
                        "0.00",
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
                bool resultado = new CN_Producto().Editar(obj, out mensaje);
                if (resultado)
                {
                    DataGridViewRow row = dgvdata.Rows[Convert.ToInt32(txtindice.Text)];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["Codigo"].Value = txtcodigo.Text;
                    row.Cells["Nombre"].Value = txtnombre.Text;
                    row.Cells["Descripcion"].Value = txtdescripcion.Text;
                    row.Cells["IdCategoria"].Value = ((OpcionCombo)cbocategoria.SelectedItem).Valor.ToString();
                    row.Cells["Categoria"].Value = ((OpcionCombo)cbocategoria.SelectedItem).Texto.ToString();

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

        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtcodigo.Text = "";
            txtnombre.Text = "";
            txtdescripcion.Text = "";
            cbocategoria.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;

            txtcodigo.Select();
        }

        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex == 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                int ancho = 20;
                int alto = 20;

                int x = e.CellBounds.Left + (e.CellBounds.Width - ancho) / 2;
                int y = e.CellBounds.Top + (e.CellBounds.Height - alto) / 2;

                e.Graphics.DrawImage(
                    imagenCheck,
                    new Rectangle(x, y, ancho, alto)
                );

                e.Handled = true;
            }
        }

        private void dgvdata_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex == dgvdata.Columns["btnseleccionar"].Index)
            {
                int indice = e.RowIndex;

                txtindice.Text = indice.ToString();
                txtid.Text = dgvdata.Rows[indice].Cells["Id"].Value.ToString();
                txtcodigo.Text = dgvdata.Rows[indice].Cells["Codigo"].Value.ToString();
                txtnombre.Text = dgvdata.Rows[indice].Cells["Nombre"].Value.ToString();
                txtdescripcion.Text = dgvdata.Rows[indice].Cells["Descripcion"].Value.ToString();
                cbocategoria.SelectedIndex = cbocategoria.FindString(dgvdata.Rows[indice].Cells["Categoria"].Value.ToString());
                cboestado.SelectedIndex = cboestado.FindString(dgvdata.Rows[indice].Cells["Estado"].Value.ToString());

                // Seleccionar CATEGORIA
                foreach (OpcionCombo oc in cbocategoria.Items)
                {
                    if (Convert.ToInt32(oc.Valor) ==
                        Convert.ToInt32(dgvdata.Rows[indice].Cells["IdCategoria"].Value))
                    {
                        cbocategoria.SelectedIndex = cbocategoria.Items.IndexOf(oc);
                        break;
                    }
                }

                // Seleccionar ESTADO
                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) ==
                        Convert.ToInt32(dgvdata.Rows[indice].Cells["EstadoValor"].Value))
                    {
                        cboestado.SelectedIndex = cboestado.Items.IndexOf(oc);
                        break;
                    }
                }
            }
        }

        private void btneliminar_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtid.Text) != 0)
            {
                if (MessageBox.Show("¿Desea eliminar el producto seleccionado?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string mensaje = string.Empty;
                    Producto obj = new Producto()
                    {
                        IdProducto = Convert.ToInt32(txtid.Text)
                    };
                    bool respuesta = new CN_Producto().Eliminar(obj, out mensaje);
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

        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
        }

        private void btnexportar_Click(object sender, EventArgs e)
        {
            // 1. Validamos si la grilla tiene al menos una fila con datos
            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("No hay datos para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                // 2. Creamos una tabla en memoria (DataTable) para estructurar los datos a exportar
                DataTable dt = new DataTable();

                // 3. Recorremos las columnas de la grilla para crear los encabezados en el DataTable
                foreach (DataGridViewColumn columna in dgvdata.Columns)
                {
                    // Solo tomamos en cuenta las columnas que tienen título y que están visibles en pantalla
                    if (columna.HeaderText != "" && columna.Visible)
                    {
                        dt.Columns.Add(columna.HeaderText, typeof(string));
                    }
                }

                // 4. Recorremos todas las filas de la grilla
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    // Filtramos solo las filas que el usuario está viendo (útil si hay filtros de búsqueda aplicados)
                    if (row.Visible)
                    {
                        // 5. Agregamos una nueva fila al DataTable volcando el contenido exacto de cada celda visible
                        dt.Rows.Add(new object[] {
                    row.Cells[2].Value.ToString(),
                    row.Cells[3].Value.ToString(),
                    row.Cells[4].Value.ToString(),
                    row.Cells[6].Value.ToString(),
                    row.Cells[7].Value.ToString(),
                    row.Cells[8].Value.ToString(),
                    row.Cells[9].Value.ToString(),
                    row.Cells[11].Value.ToString()
                });
                    }
                }

                // 6. Configuramos la clásica ventana de Windows de "Guardar como..."
                SaveFileDialog savefile = new SaveFileDialog();
                // Genera un nombre de archivo automático con la fecha y hora para evitar sobreescribir reportes anteriores
                savefile.FileName = string.Format("ReporteProducto_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                // Filtramos para que solo permita guardar con formato Excel (.xlsx)
                savefile.Filter = "Excel Files | *.xlsx";

                // 7. Abrimos la ventana. Si el usuario elige una ruta y le da a "Guardar" (DialogResult.OK), entramos al if
                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 8. Creamos el archivo Excel en blanco usando la librería ClosedXML
                        XLWorkbook wb = new XLWorkbook();
                        // Le agregamos una hoja llamada "Informe" y le pasamos de golpe todo nuestro DataTable (dt)
                        var hoja = wb.Worksheets.Add(dt, "Informe");
                        // Ajustamos el ancho de las columnas automáticamente para que el texto no quede escondido
                        hoja.ColumnsUsed().AdjustToContents();

                        // Guardamos el archivo físico en la ruta elegida por el usuario
                        wb.SaveAs(savefile.FileName);

                        // Avisamos que salió todo bien
                        MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        // Si ocurre un error (por ejemplo, el usuario tiene un archivo con el mismo nombre abierto en Excel), mostramos error
                        MessageBox.Show("Error al generar reporte", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

// LIBRERÍA PARA VALIDAR EL FORMATO DEL CORREO
using System.Text.RegularExpressions;

// NUEVO: LIBRERÍA PARA ESTANDARIZAR MAYÚSCULAS Y MINÚSCULAS (TitleCase)
using System.Globalization;

using CapaPresentacion.Utilidades;
using CapaEntidad;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class frmUsuarios : Form
    {
        private Image imagenCheck;

        public frmUsuarios()
        {
            InitializeComponent();

            // Buscamos la ruta de las imágenes para el botón de seleccionar en la grilla
            string carpetaImagenes = Path.Combine(
                 Directory.GetParent(Application.StartupPath).Parent.Parent.FullName,
                 "CapaPresentacion",
                 "Images"
            );
            imagenCheck = Image.FromFile(Path.Combine(carpetaImagenes, "check20.png"));
        }

        private void frmUsuarios_Load(object sender, EventArgs e)
        {
            // Vinculamos la validación para que el Documento solo acepte números
            txtdocumento.KeyPress += new KeyPressEventHandler(ValidarNumeros_KeyPress);

            // --- LLENAR COMBOS (DESPLEGABLES) ---
            cboestado.Items.Add(new OpcionCombo() { Valor = 1, Texto = "Activo" });
            cboestado.Items.Add(new OpcionCombo() { Valor = 0, Texto = "No Activo" });
            cboestado.DisplayMember = "Texto";
            cboestado.ValueMember = "Valor";
            cboestado.SelectedIndex = 0;

            // Traemos los roles de la BD
            List<Rol> listaRol = new CN_Rol().Listar();
            foreach (Rol item in listaRol)
            {
                cborol.Items.Add(new OpcionCombo() { Valor = item.IdRol, Texto = item.Descripcion });
            }
            cborol.DisplayMember = "Texto";
            cborol.ValueMember = "Valor";
            cborol.SelectedIndex = 0;

            // Llenamos el combo de búsqueda
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

            // --- LLENAR GRILLA DE USUARIOS ---
            List<Usuario> listaUsuario = new CN_Usuario().Listar();
            foreach (Usuario item in listaUsuario)
            {
                string apellidoYNombre = item.Apellido + ", " + item.Nombre;

                dgvdata.Rows.Add(new object[] {"", item.IdUsuario, item.Documento, apellidoYNombre, item.Correo, item.Clave ,
                    item.oRol.IdRol,
                    item.oRol.Descripcion,
                    item.Estado == true ? 1 : 0,
                    item.Estado == true ? "Activo" : "No Activo"
                });
            }
        }

        // --- VALIDACIONES DE TECLADO ---
        private void ValidarNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // --- NUEVO: FUNCIÓN PARA ESTANDARIZAR MAYÚSCULAS Y MINÚSCULAS (Title Case) ---
        private string FormatearTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(texto.Trim().ToLower());
        }

        // --- VALIDACIÓN DE FORMATO DE CORREO ---
        private bool EsCorreoValido(string correo)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex regex = new Regex(patron);
            return regex.IsMatch(correo);
        }

        // --- VALIDACIÓN DE CONTRASEÑA SEGURA ---
        private bool EsClaveSegura(string clave)
        {
            if (clave.Length < 8) return false;

            bool tieneMayuscula = false, tieneMinuscula = false, tieneNumero = false;

            foreach (char c in clave)
            {
                if (char.IsUpper(c)) tieneMayuscula = true;
                if (char.IsLower(c)) tieneMinuscula = true;
                if (char.IsDigit(c)) tieneNumero = true;
            }

            return tieneMayuscula && tieneMinuscula && tieneNumero;
        }

        // --- BOTÓN GUARDAR ---
        private void btnguardar_Click(object sender, EventArgs e)
        {
            // 1. Validar que no haya campos vacíos
            if (string.IsNullOrWhiteSpace(txtdocumento.Text) ||
                string.IsNullOrWhiteSpace(txtnombre.Text) ||
                string.IsNullOrWhiteSpace(txtapellido.Text) ||
                string.IsNullOrWhiteSpace(txtcorreo.Text) ||
                string.IsNullOrWhiteSpace(txtclave.Text))
            {
                MessageBox.Show("Debe completar todos los campos obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validar formato del correo
            if (!EsCorreoValido(txtcorreo.Text))
            {
                MessageBox.Show("El formato del correo electrónico no es válido.\nEjemplo correcto: usuario@dominio.com", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtcorreo.Focus();
                return;
            }

            // 3. Validar coincidencia de contraseñas
            if (txtclave.Text != txtconfirmarclave.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden. Por favor, verifique.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtconfirmarclave.Focus();
                return;
            }

            // 4. Validar seguridad de la contraseña
            if (!EsClaveSegura(txtclave.Text))
            {
                MessageBox.Show("La contraseña es muy débil.\n\nDebe tener al menos:\n- 8 caracteres\n- Una letra mayúscula\n- Una letra minúscula\n- Un número", "Seguridad requerida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtclave.Focus();
                return;
            }

            string mensaje = string.Empty;

            // Creamos el objeto aplicando el formato prolijo (Title Case) en Nombre y Apellido
            Usuario objusuario = new Usuario()
            {
                IdUsuario = Convert.ToInt32(txtid.Text),
                Documento = txtdocumento.Text,
                Nombre = FormatearTexto(txtnombre.Text),       // <-- ESTANDARIZADO
                Apellido = FormatearTexto(txtapellido.Text),   // <-- ESTANDARIZADO
                Correo = txtcorreo.Text.Trim().ToLower(),      // <-- Correo siempre en minúsculas
                Clave = txtclave.Text,
                oRol = new Rol() { IdRol = Convert.ToInt32(((OpcionCombo)cborol.SelectedItem).Valor) },
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false
            };

            if (objusuario.IdUsuario == 0)
            {
                int idusuariogenerado = new CN_Usuario().Registrar(objusuario, out mensaje);

                if (idusuariogenerado != 0)
                {
                    string apellidoYNombre = objusuario.Apellido + ", " + objusuario.Nombre;

                    dgvdata.Rows.Add(new object[] {"",idusuariogenerado, txtdocumento.Text, apellidoYNombre, txtcorreo.Text, txtclave.Text,
                        ((OpcionCombo)cborol.SelectedItem).Valor.ToString(),
                        ((OpcionCombo)cborol.SelectedItem).Texto.ToString(),
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
                bool resultado = new CN_Usuario().Editar(objusuario, out mensaje);
                if (resultado)
                {
                    string apellidoYNombre = objusuario.Apellido + ", " + objusuario.Nombre;

                    DataGridViewRow row = dgvdata.Rows[Convert.ToInt32(txtindice.Text)];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["Documento"].Value = txtdocumento.Text;
                    row.Cells["NombreCompleto"].Value = apellidoYNombre;
                    row.Cells["Correo"].Value = txtcorreo.Text;
                    row.Cells["Clave"].Value = txtclave.Text;
                    row.Cells["IdRol"].Value = ((OpcionCombo)cborol.SelectedItem).Valor.ToString();
                    row.Cells["Rol"].Value = ((OpcionCombo)cborol.SelectedItem).Texto.ToString();
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

        private void btneliminar_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtid.Text) != 0)
            {
                if (MessageBox.Show("¿Desea eliminar el usuario seleccionado?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string mensaje = string.Empty;
                    Usuario objusuario = new Usuario() { IdUsuario = Convert.ToInt32(txtid.Text) };

                    bool respuesta = new CN_Usuario().Eliminar(objusuario, out mensaje);
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

        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtdocumento.Text = "";
            txtnombre.Text = "";
            txtapellido.Text = "";
            txtcorreo.Text = "";
            txtclave.Text = "";
            txtconfirmarclave.Text = "";
            cborol.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;
            txtdocumento.Select();
        }

        private void btneditar_Click(object sender, EventArgs e)
        {
            Limpiar();
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

        private void dgvdata_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgvdata.Columns["btnseleccionar"].Index)
            {
                int indice = e.RowIndex;

                txtindice.Text = indice.ToString();
                txtid.Text = dgvdata.Rows[indice].Cells["Id"].Value.ToString();
                txtdocumento.Text = dgvdata.Rows[indice].Cells["Documento"].Value.ToString();

                string nombreCompletoGrilla = dgvdata.Rows[indice].Cells["NombreCompleto"].Value.ToString();
                string[] partes = nombreCompletoGrilla.Split(',');
                if (partes.Length == 2)
                {
                    txtapellido.Text = partes[0].Trim();
                    txtnombre.Text = partes[1].Trim();
                }

                txtcorreo.Text = dgvdata.Rows[indice].Cells["Correo"].Value.ToString();
                txtclave.Text = dgvdata.Rows[indice].Cells["Clave"].Value.ToString();
                txtconfirmarclave.Text = dgvdata.Rows[indice].Cells["Clave"].Value.ToString();

                foreach (OpcionCombo oc in cboestado.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(dgvdata.Rows[indice].Cells["EstadoValor"].Value))
                    {
                        cboestado.SelectedIndex = cboestado.Items.IndexOf(oc);
                        break;
                    }
                }
                foreach (OpcionCombo oc in cborol.Items)
                {
                    if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(dgvdata.Rows[indice].Cells["IdRol"].Value))
                    {
                        cborol.SelectedIndex = cborol.Items.IndexOf(oc);
                        break;
                    }
                }
            }
        }

        // Eventos vacíos que no se pueden borrar
        private void label1_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void txtcorreo_TextChanged(object sender, EventArgs e) { }
        private void txtdocumento_TextChanged(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void txtconfirmarclave_TextChanged(object sender, EventArgs e) { }
        private void label9_Click(object sender, EventArgs e) { }
        private void cboestado_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cborol_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
        private void txtclave_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void txtnombrecompleto_TextChanged(object sender, EventArgs e) { }
    }
}
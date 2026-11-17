using CapaEntidad;
using CapaNegocio;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario para consultar comprobantes de compra registrados por número de documento
    /// y exportar su detalle en formato PDF mediante una plantilla HTML.
    /// </summary>
    public partial class frmDetalleCompra : Form
    {
        private readonly CultureInfo culturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Inicializa el formulario y enlaza el evento de carga inicial.
        /// </summary>
        public frmDetalleCompra()
        {
            InitializeComponent();
            this.Load += frmDetalleCompra_Load;
        }

        /// <summary>
        /// Configura los cuadros de texto en modo solo lectura, ajusta la alineación
        /// de las columnas de la grilla y prepara el buscador al abrir el formulario.
        /// </summary>
        private void frmDetalleCompra_Load(object sender, EventArgs e)
        {
            // Bloquear los campos informativos para evitar edición manual
            TextBox[] camposSoloLectura = {
                txtnumerodocumento, txtfecha, txttipodocumento, txtusuario,
                txtdocproveedor, txtnombreproveedor, txtmontototal
            };

            foreach (TextBox txt in camposSoloLectura)
            {
                if (txt != null)
                {
                    txt.ReadOnly = true;
                    txt.TabStop = false;
                    txt.BackColor = Color.WhiteSmoke;
                }
            }

            // Configurar la grilla para evitar edición y ajustar el ancho de columnas numéricas
            dgvdata.ReadOnly = true;
            dgvdata.AllowUserToAddRows = false;

            if (dgvdata.Columns.Count >= 4)
            {
                dgvdata.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvdata.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                dgvdata.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dgvdata.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvdata.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Permitir ejecutar la búsqueda presionando la tecla Enter
            txtbusqueda.KeyDown += txtbusqueda_KeyDown;

            LimpiarCampos();
            txtbusqueda.Select();
        }

        /// <summary>
        /// Convierte un monto decimal a texto con separador de miles en formato argentino (ej. 15.000).
        /// </summary>
        private string FormatearMonedaAR(decimal valor)
        {
            return valor.ToString("N0", culturaAR);
        }

        /// <summary>
        /// Reemplaza caracteres especiales (&amp;, &lt;, &gt;) para evitar errores al interpretar la plantilla HTML.
        /// </summary>
        private string EscaparHtml(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        /// <summary>
        /// Detecta la tecla Enter en el cuadro de búsqueda y ejecuta el botón Buscar.
        /// </summary>
        private void txtbusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnbuscar.PerformClick();
            }
        }

        /// <summary>
        /// Busca la compra por su número de comprobante y muestra su cabecera y productos en pantalla.
        /// </summary>
        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string busqueda = txtbusqueda.Text.Trim();

            if (string.IsNullOrEmpty(busqueda))
            {
                MessageBox.Show("Ingrese un número de documento para buscar.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtbusqueda.Select();
                return;
            }

            // Si el usuario ingresa un número corto (ej: "1"), completarlo con ceros a 5 dígitos ("00001")
            if (busqueda.All(char.IsDigit) && busqueda.Length < 5)
            {
                busqueda = busqueda.PadLeft(5, '0');
                txtbusqueda.Text = busqueda;
            }

            // Consultar cabecera y detalle de la compra en la capa de negocio
            Compra oCompra = new CN_Compra().ObtenerCompra(busqueda);

            if (oCompra != null && oCompra.IdCompra != 0)
            {
                // Cargar datos de cabecera
                txtnumerodocumento.Text = oCompra.NumeroDocumento;
                txtfecha.Text = oCompra.FechaRegistro;
                txttipodocumento.Text = oCompra.TipoDocumento;

                txtusuario.Text = oCompra.oUsuario != null
                    ? $"{oCompra.oUsuario.Nombre} {oCompra.oUsuario.Apellido}".Trim()
                    : "";

                txtdocproveedor.Text = oCompra.oProveedor != null ? oCompra.oProveedor.Documento : "";
                txtnombreproveedor.Text = oCompra.oProveedor != null ? oCompra.oProveedor.RazonSocial : "";

                // Cargar ítems en la grilla de detalle
                dgvdata.Rows.Clear();

                if (oCompra.oDetalleCompra != null)
                {
                    foreach (Detalle_Compra dc in oCompra.oDetalleCompra)
                    {
                        dgvdata.Rows.Add(new object[] {
                            dc.oProducto.Nombre,
                            FormatearMonedaAR(dc.PrecioCompra),
                            dc.Cantidad,
                            FormatearMonedaAR(dc.MontoTotal)
                        });
                    }
                }

                txtmontototal.Text = FormatearMonedaAR(oCompra.MontoTotal);
            }
            else
            {
                LimpiarCampos();
                MessageBox.Show("No se encontraron resultados para el número de documento ingresado.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtbusqueda.Select();
            }
        }

        /// <summary>
        /// Vacía todos los campos informativos y limpia las filas de la grilla.
        /// </summary>
        private void LimpiarCampos()
        {
            txtnumerodocumento.Text = "";
            txtfecha.Text = "";
            txttipodocumento.Text = "";
            txtusuario.Text = "";
            txtdocproveedor.Text = "";
            txtnombreproveedor.Text = "";

            dgvdata.Rows.Clear();
            txtmontototal.Text = "0";
        }

        /// <summary>
        /// Limpia el filtro de búsqueda, restablece el formulario y devuelve el foco al buscador.
        /// </summary>
        private void btnborrar_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            LimpiarCampos();
            txtbusqueda.Select();
        }

        /// <summary>
        /// Genera y guarda un archivo PDF con el comprobante de compra utilizando PlantillaCompra.html e iTextSharp.
        /// </summary>
        private void btndescargar_Click(object sender, EventArgs e)
        {
            // 1. Verificar que haya una compra cargada en pantalla antes de exportar
            if (string.IsNullOrWhiteSpace(txttipodocumento.Text) || dgvdata.Rows.Count == 0)
            {
                MessageBox.Show("No se encontraron resultados para exportar.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Localizar y leer la plantilla HTML base del comprobante
            string rutaPlantilla = Path.Combine(
                Directory.GetParent(Application.StartupPath).Parent.Parent.FullName,
                "CapaPresentacion",
                "Plantillas",
                "PlantillaCompra.html"
            );

            if (!File.Exists(rutaPlantilla))
            {
                MessageBox.Show("No se encontró la plantilla HTML en:\n" + rutaPlantilla, "Archivo no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string Texto_Html = File.ReadAllText(rutaPlantilla);

            // 3. Reemplazar marcadores con datos del negocio, cabecera y filas de productos
            Negocio odatos = new CN_Negocio().ObtenerDatos();

            Texto_Html = Texto_Html.Replace("@nombrenegocio", EscaparHtml(odatos?.Nombre?.ToUpper() ?? ""));
            Texto_Html = Texto_Html.Replace("@docnegocio", EscaparHtml(odatos?.RUC ?? ""));
            Texto_Html = Texto_Html.Replace("@direcnegocio", EscaparHtml(odatos?.Direccion ?? ""));

            Texto_Html = Texto_Html.Replace("@tipodocumento", EscaparHtml(txttipodocumento.Text.ToUpper()));
            Texto_Html = Texto_Html.Replace("@numerodocumento", EscaparHtml(txtnumerodocumento.Text));
            Texto_Html = Texto_Html.Replace("@docproveedor", EscaparHtml(txtdocproveedor.Text));
            Texto_Html = Texto_Html.Replace("@nombreproveedor", EscaparHtml(txtnombreproveedor.Text));
            Texto_Html = Texto_Html.Replace("@fecharegistro", EscaparHtml(txtfecha.Text));
            Texto_Html = Texto_Html.Replace("@usuarioregistro", EscaparHtml(txtusuario.Text));

            string filas = string.Empty;
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                filas += "<tr>";
                filas += "<td>" + EscaparHtml(row.Cells["Producto"].Value?.ToString()) + "</td>";
                filas += "<td>" + EscaparHtml(row.Cells["PrecioCompra"].Value?.ToString()) + "</td>";
                filas += "<td>" + EscaparHtml(row.Cells["Cantidad"].Value?.ToString()) + "</td>";
                filas += "<td>" + EscaparHtml(row.Cells["SubTotal"].Value?.ToString()) + "</td>";
                filas += "</tr>";
            }

            Texto_Html = Texto_Html.Replace("@filas", filas);
            Texto_Html = Texto_Html.Replace("@montototal", EscaparHtml(txtmontototal.Text));

            // 4. Solicitar ruta de guardado y renderizar el HTML junto con el logo a PDF
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = string.Format("Compra_{0}.pdf", txtnumerodocumento.Text);
            savefile.Filter = "Pdf Files|*.pdf";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Insertar el logo del negocio en la esquina superior izquierda si está configurado
                    bool obtenido = true;
                    byte[] byteImage = new CN_Negocio().ObtenerLogo(out obtenido);

                    if (obtenido && byteImage != null && byteImage.Length > 0)
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(byteImage);
                        img.ScaleToFit(60, 60);
                        img.Alignment = iTextSharp.text.Image.UNDERLYING;
                        img.SetAbsolutePosition(pdfDoc.Left, pdfDoc.GetTop(51));
                        pdfDoc.Add(img);
                    }

                    using (StringReader sr = new StringReader(Texto_Html))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    }

                    pdfDoc.Close();
                    MessageBox.Show("Documento Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}